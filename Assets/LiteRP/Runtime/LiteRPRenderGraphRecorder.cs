using System;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder : IRenderGraphRecorder, IDisposable
    {
        private static readonly ShaderTagId[] s_shaderTagIds = new ShaderTagId[]
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("LiteRPForward")
        }; //渲染标签IDs
        
        private const string k_BackBufferColorTextureName = "_BackBufferColor";
        private const string k_BackBufferDepthTextureName = "_BackBufferDepth";
        private RTHandle m_ColorTarget = null;
        private RTHandle m_DepthTarget = null;

        internal LiteRPRenderGraphRecorder()
        {
            InitializeMainLightShadowMapPass();
        }

        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            RenderTargetData renderTargetData = frameData.Get<RenderTargetData>();
            CameraData cameraData = frameData.Get<CameraData>();
            LightData lightData = frameData.Get<LightData>();
            ShadowData shadowData = frameData.Get<ShadowData>();
            renderTargetData.InitFrame();
            
            AddSetupLightsPass(renderGraph, cameraData, lightData);
            CreateRenderGraphCameraRenderTargets(renderGraph, renderTargetData, cameraData);
            AddInitRenderGraphFramePass(renderGraph);
            AddSetupCameraPropertiesPass(renderGraph, cameraData);

            if (NeedMainLightShadowMapPass(cameraData, lightData, shadowData))
            {
                AddDrawMainLightShadowMapPass(renderGraph, renderTargetData, cameraData, lightData, shadowData);
                AddSetupCameraPropertiesPass(renderGraph, cameraData);
            }

            CameraClearFlags clearFlags = cameraData.camera.clearFlags;
            if(!renderGraph.nativeRenderPassesEnabled && clearFlags != CameraClearFlags.Nothing)
            {
                AddClearRenderTargetPass(renderGraph, renderTargetData, cameraData);
            }
            AddDrawOpaqueObjectsPass(renderGraph, renderTargetData, cameraData);
            if(clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                AddDrawSkyBoxPass(renderGraph, renderTargetData,cameraData);
            }
            AddDrawTransparentObjectsPass(renderGraph, renderTargetData, cameraData);
            
#if UNITY_EDITOR
            AddDrawEditorGizmoPass(renderGraph, renderTargetData, cameraData, GizmoSubset.PreImageEffects);
            AddDrawEditorGizmoPass(renderGraph, renderTargetData, cameraData, GizmoSubset.PostImageEffects);
#endif
            renderTargetData.EndFrame();
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            var targetTexture = cameraData.camera.targetTexture;
            var cameraTargetTexture = targetTexture;
            bool isBuildInTexture = (cameraTargetTexture == null);
            bool isCameraTargetOffscreenDepth = !isBuildInTexture && targetTexture.format == RenderTextureFormat.Depth;
            
            RenderTargetIdentifier targetColorId = isBuildInTexture
                ? BuiltinRenderTextureType.CameraTarget
                : new RenderTargetIdentifier(cameraTargetTexture);
            if(m_ColorTarget == null)
                m_ColorTarget = RTHandles.Alloc((RenderTargetIdentifier)targetColorId, k_BackBufferColorTextureName);
            else if(m_ColorTarget.nameID != targetColorId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_ColorTarget, targetColorId);

            RenderTargetIdentifier targetDepthId = isBuildInTexture
                ? BuiltinRenderTextureType.Depth
                : new RenderTargetIdentifier(cameraTargetTexture);
            if(m_DepthTarget == null)
                m_DepthTarget = RTHandles.Alloc((RenderTargetIdentifier)targetDepthId, k_BackBufferDepthTextureName);
            else if(m_DepthTarget.nameID != targetDepthId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_DepthTarget, targetDepthId);
            
            Color clearColor = cameraData.GetClearColor();
            RTClearFlags clearFlags = cameraData.GetClearFlags();
            
            bool clearOnFirstUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardColorBackbufferOnLastUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardDepthBackbufferOnLastUse = !isCameraTargetOffscreenDepth;
            
            ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
            importBackbufferColorParams.clearOnFirstUse = clearOnFirstUse;
            importBackbufferColorParams.clearColor = clearColor;
            importBackbufferColorParams.discardOnLastUse = discardColorBackbufferOnLastUse;
            
            ImportResourceParams importBackbufferDepthParams = new ImportResourceParams();
            importBackbufferDepthParams.clearOnFirstUse = clearOnFirstUse;
            importBackbufferDepthParams.clearColor = clearColor;
            importBackbufferDepthParams.discardOnLastUse = discardDepthBackbufferOnLastUse;
#if UNITY_EDITOR
            // on TBDR GPUs like Apple M1/M2, we need to preserve the backbuffer depth for overlay cameras in Editor for Gizmos
            if (cameraData.camera.cameraType == CameraType.SceneView)
                importBackbufferDepthParams.discardOnLastUse = false;
#endif
            
            bool colorRT_sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            RenderTargetInfo importInfoColor = new RenderTargetInfo();
            RenderTargetInfo importInfoDepth = new RenderTargetInfo();
            if (isBuildInTexture)
            {
                importInfoColor.width = Screen.width;
                importInfoColor.height = Screen.height;
                importInfoColor.volumeDepth = 1;
                importInfoColor.msaaSamples = 1;
                importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);
                importInfoColor.bindMS = false;
            
                importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            }
            else
            {
                importInfoColor.width = cameraTargetTexture.width;
                importInfoColor.height = cameraTargetTexture.height;
                importInfoColor.volumeDepth = cameraTargetTexture.volumeDepth;
                importInfoColor.msaaSamples = cameraTargetTexture.antiAliasing;
                importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);
                importInfoColor.bindMS = false;
            
                importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            }
            
            renderTargetData.backBufferColor = renderGraph.ImportTexture(m_ColorTarget, importInfoColor, importBackbufferColorParams);
            renderTargetData.backBufferDepth = renderGraph.ImportTexture(m_DepthTarget, importInfoDepth, importBackbufferDepthParams);
        }
        
        
        
        public void Dispose()
        {
            ReleaseMainLightShadowMapPass();
            
            RTHandles.Release(m_ColorTarget);
            RTHandles.Release(m_DepthTarget);
            GC.SuppressFinalize(this);
        }
    }
}