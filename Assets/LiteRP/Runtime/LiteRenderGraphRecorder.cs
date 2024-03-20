using System;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder : IRenderGraphRecorder, IDisposable
    {
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); //渲染标签ID
        
        private TextureHandle m_BackbufferColorHandle = TextureHandle.nullHandle;
        private RTHandle m_TargetColorHandle = null;
        
        private TextureHandle m_BackbufferDepthHandle = TextureHandle.nullHandle;
        private RTHandle m_TargetDepthHandle = null;
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            CameraData cameraData = frameData.Get<CameraData>();
            CreateRenderGraphCameraRenderTargets(renderGraph, cameraData);
            AddSetupCameraPropertiesPass(renderGraph, cameraData);
            CameraClearFlags clearFlags = cameraData.camera.clearFlags;
            if (!renderGraph.nativeRenderPassesEnabled)
            {
                if(clearFlags != CameraClearFlags.Nothing)
                {
                    AddClearRenderTargetPass(renderGraph, cameraData);
                }
            }
            AddDrawOpaqueObjectsPass(renderGraph, cameraData);
            if(clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                AddDrawSkyBoxPass(renderGraph, cameraData);
            }
            AddDrawTransparentObjectsPass(renderGraph, cameraData);
            //临时写法
#if UNITY_EDITOR
            //if (cameraData.camera.cameraType == CameraType.SceneView)
            //    AddEditorRenderTargetPass(renderGraph);
            AddDrawEditorGizmoPass(renderGraph, cameraData, GizmoSubset.PreImageEffects);
            AddDrawEditorGizmoPass(renderGraph, cameraData, GizmoSubset.PostImageEffects);
#endif
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, CameraData cameraData)
        {
            var cameraTargetTexture = cameraData.camera.targetTexture;
            bool isBuiltInTexture = (cameraTargetTexture == null);
            bool isCameraTargetOffscreenDepth = !isBuiltInTexture && cameraData.camera.targetTexture.format == RenderTextureFormat.Depth;
            
            RenderTargetIdentifier targetColorId = isBuiltInTexture? BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier(cameraTargetTexture);
            RenderTargetIdentifier targetDepthId = isBuiltInTexture ? BuiltinRenderTextureType.Depth : new RenderTargetIdentifier(cameraTargetTexture);
            
            if(m_TargetColorHandle == null)
                m_TargetColorHandle = RTHandles.Alloc((RenderTargetIdentifier)targetColorId, "BackBuffer color");
            else if(m_TargetColorHandle.nameID != targetColorId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetColorHandle, targetColorId);
            
            if (m_TargetDepthHandle == null)
                m_TargetDepthHandle = RTHandles.Alloc(targetDepthId, "Backbuffer depth");
            else if (m_TargetDepthHandle.nameID != targetDepthId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetDepthHandle, targetDepthId);

            Color clearColor = cameraData.GetClearColor();
            bool clearBackbufferOnFirstUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardColorBackbufferOnLastUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardDepthBackbufferOnLastUse = !isCameraTargetOffscreenDepth;
            
            ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
            importBackbufferColorParams.clearOnFirstUse = clearBackbufferOnFirstUse;
            importBackbufferColorParams.clearColor = clearColor;
            importBackbufferColorParams.discardOnLastUse = discardColorBackbufferOnLastUse;
            
            ImportResourceParams importBackbufferDepthParams = new ImportResourceParams();
            importBackbufferDepthParams.clearOnFirstUse = clearBackbufferOnFirstUse;
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
            if (isBuiltInTexture)
            {
                importInfoColor.width = Screen.width;
                importInfoColor.height = Screen.height;
                importInfoColor.volumeDepth = 1;
                importInfoColor.msaaSamples = 1;
                importInfoColor.format =
                    GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);
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

                importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            }

            m_BackbufferColorHandle = renderGraph.ImportTexture(m_TargetColorHandle, importInfoColor, importBackbufferColorParams);
            m_BackbufferDepthHandle = renderGraph.ImportTexture(m_TargetDepthHandle, importInfoDepth, importBackbufferDepthParams);
        }

        public void Dispose()
        {
            RTHandles.Release(m_TargetColorHandle);
            RTHandles.Release(m_TargetDepthHandle);
            GC.SuppressFinalize(this);
        }
    }
}