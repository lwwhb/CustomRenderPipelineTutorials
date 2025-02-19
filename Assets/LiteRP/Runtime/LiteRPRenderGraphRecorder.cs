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
        
        private static bool m_RequiresIntermediateAttachments = false;
        private RTHandle m_ColorTarget = null;
        private RTHandle m_DepthTarget = null;
        private RTHandle m_CameraColorAttachment = null;
        private RTHandle m_CameraDepthAttachment = null;
        
        //Engine Materials
        private Material m_BlitMaterial = null;
        private Material m_BlitHDRMaterial = null;
        private Material m_SamplingMaterial = null;
        
        private Material m_CopyDepthMaterial = null;
        //---
        
        
        internal LiteRPRenderGraphRecorder()
        {
            InitializeEngineMaterials();
            InitializeMainLightShadowMapPass();
        }

        private void InitializeEngineMaterials()
        {
            if (GraphicsSettings.TryGetRenderPipelineSettings<LiteRPRuntimeShaders>(
                    out var shadersResources))
            {
                m_BlitMaterial = CoreUtils.CreateEngineMaterial(shadersResources.coreBlitPS);
                m_BlitHDRMaterial = CoreUtils.CreateEngineMaterial(shadersResources.blitHDROverlay);
                m_SamplingMaterial = CoreUtils.CreateEngineMaterial(shadersResources.samplingPS);
                
                m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(shadersResources.copyDepthPS);
            }
        }

        private void ReleaseEngineMaterials()
        {
            CoreUtils.Destroy(m_BlitMaterial);
            CoreUtils.Destroy(m_BlitHDRMaterial);
            CoreUtils.Destroy(m_SamplingMaterial);
            
            CoreUtils.Destroy(m_CopyDepthMaterial);
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
            if (NeedCopyDepthPass(cameraData))
            {
                AddCopyDepthPass(renderGraph, renderTargetData, cameraData);
            }
            if(clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                AddDrawSkyBoxPass(renderGraph, renderTargetData,cameraData);
            }
            AddDrawTransparentObjectsPass(renderGraph, renderTargetData, cameraData);

            if (NeedFinalBlitPass())
            {
                AddFinalBlitPass(renderGraph, renderTargetData, cameraData);
            }
            
#if UNITY_EDITOR
            AddDrawEditorGizmoPass(renderGraph, renderTargetData, cameraData, GizmoSubset.PreImageEffects);
            AddDrawEditorGizmoPass(renderGraph, renderTargetData, cameraData, GizmoSubset.PostImageEffects);
#endif
            renderTargetData.EndFrame();
        }
        
        bool RequiresIntermediateAttachments(CameraData cameraData)
        {
            var requireColorTexture = false;
            requireColorTexture |= Application.isEditor;

            var requireDepthTexture = true;
            
            // 因为Intermediate texture有不同的yflip状态，所以如果使用Intermediate texture，我们必须同时使用color和depth。
            return (requireColorTexture || requireDepthTexture);
        }
        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            m_RequiresIntermediateAttachments = RequiresIntermediateAttachments(cameraData);
            m_RequiresIntermediateAttachments = false;
            
            var targetTexture = cameraData.camera.targetTexture;
            var cameraTargetTexture = targetTexture;
            bool isBuildInTexture = (cameraTargetTexture == null);
            bool isCameraTargetOffscreenDepth = !isBuildInTexture && targetTexture.format == RenderTextureFormat.Depth;
            
            RenderTargetIdentifier targetColorId = isBuildInTexture
                ? BuiltinRenderTextureType.CameraTarget
                : new RenderTargetIdentifier(cameraTargetTexture);
            if(m_ColorTarget == null)
                m_ColorTarget = RTHandles.Alloc((RenderTargetIdentifier)targetColorId, ShaderPropertyName.backBufferColorTextureName);
            else if(m_ColorTarget.nameID != targetColorId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_ColorTarget, targetColorId);

            RenderTargetIdentifier targetDepthId = isBuildInTexture
                ? BuiltinRenderTextureType.Depth
                : new RenderTargetIdentifier(cameraTargetTexture);
            if(m_DepthTarget == null)
                m_DepthTarget = RTHandles.Alloc((RenderTargetIdentifier)targetDepthId, ShaderPropertyName.backBufferDepthTextureName);
            else if(m_DepthTarget.nameID != targetDepthId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_DepthTarget, targetDepthId);
            
            Color clearColor = cameraData.GetClearColor();
            RTClearFlags clearFlags = cameraData.GetClearFlags();
            
            bool clearOnFirstUse = !renderGraph.nativeRenderPassesEnabled && !m_RequiresIntermediateAttachments;
            bool discardColorBackbufferOnLastUse = !renderGraph.nativeRenderPassesEnabled && !m_RequiresIntermediateAttachments;
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
            renderTargetData.activeColorID = ResourceData.ActiveID.BackBuffer;
            renderTargetData.backBufferDepth = renderGraph.ImportTexture(m_DepthTarget, importInfoDepth, importBackbufferDepthParams);
            renderTargetData.activeDepthID = ResourceData.ActiveID.BackBuffer;

            //检查是否需要使用Intermediate texture的FrontBuffer
            if (m_RequiresIntermediateAttachments)   
            {
                Color cameraBackgroundColor = (cameraData.camera.clearFlags == CameraClearFlags.Nothing && cameraData.camera.targetTexture == null) ? Color.yellow :clearColor;
                ImportResourceParams importColorParams = new ImportResourceParams();
                importColorParams.clearOnFirstUse = cameraData.camera.clearFlags != CameraClearFlags.Nothing;
                importColorParams.clearColor = cameraBackgroundColor;
                importColorParams.discardOnLastUse = false;

                ImportResourceParams importDepthParams = new ImportResourceParams();
                importDepthParams.clearOnFirstUse = cameraData.camera.cameraType == CameraType.SceneView ? true : false;
                importDepthParams.clearColor = cameraBackgroundColor;
                importDepthParams.discardOnLastUse = false;
                
                // lwwhb: 暂时不支持MSAA
                RenderTextureDescriptor cameraRTDescriptor = cameraData.cameraTargetDescriptor;
                cameraRTDescriptor.useMipMap = false;
                cameraRTDescriptor.autoGenerateMips = false;
                cameraRTDescriptor.depthStencilFormat  = GraphicsFormat.None;;
                
                RenderingUtils.ReAllocateHandleIfNeeded(ref m_CameraColorAttachment, cameraRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: ShaderPropertyName.cameraColorAttachmentName);
                renderTargetData.frontBufferColor = renderGraph.ImportTexture(m_CameraColorAttachment, importColorParams);
                renderTargetData.activeColorID = ResourceData.ActiveID.FrontBuffer;
                
                cameraRTDescriptor = cameraData.cameraTargetDescriptor;
                cameraRTDescriptor.useMipMap = false;
                cameraRTDescriptor.autoGenerateMips = false;
                
                cameraRTDescriptor.graphicsFormat = GraphicsFormat.None;;
                cameraRTDescriptor.depthStencilFormat = CoreUtils.GetDefaultDepthStencilFormat();
                RenderingUtils.ReAllocateHandleIfNeeded(ref m_CameraDepthAttachment, cameraRTDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: ShaderPropertyName.cameraDepthAttachmentName);
                importBackbufferDepthParams.discardOnLastUse = discardDepthBackbufferOnLastUse;
#if UNITY_EDITOR
                // scene filtering will reuse "camera" depth  from the normal pass for the "filter highlight" effect
                if (cameraData.camera.cameraType == CameraType.SceneView && CoreUtils.IsSceneFilteringEnabled())
                    importDepthParams.discardOnLastUse = false;
#endif
                renderTargetData.frontBufferDepth = renderGraph.ImportTexture(m_CameraDepthAttachment, importDepthParams);
                renderTargetData.activeDepthID = ResourceData.ActiveID.FrontBuffer;
            }
        }
        
        public void Dispose()
        {
            ReleaseEngineMaterials();
            RTHandles.Release(m_ColorTarget);
            RTHandles.Release(m_DepthTarget);
            RTHandles.Release(m_CameraColorAttachment);
            RTHandles.Release(m_CameraDepthAttachment);
            GC.SuppressFinalize(this);
        }
    }
}