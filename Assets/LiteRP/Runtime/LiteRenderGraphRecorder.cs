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
            if(!renderGraph.nativeRenderPassesEnabled && clearFlags != CameraClearFlags.Nothing)
            {
                AddClearRenderTargetPass(renderGraph, cameraData);
            }
            AddDrawOpaqueObjectsPass(renderGraph, cameraData);
            if(clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                AddDrawSkyBoxPass(renderGraph, cameraData);
            }
            AddDrawTransparentObjectsPass(renderGraph, cameraData);
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, CameraData cameraData)
        {
            RenderTargetIdentifier targetColorId = BuiltinRenderTextureType.CameraTarget;
            if(m_TargetColorHandle == null)
                m_TargetColorHandle = RTHandles.Alloc((RenderTargetIdentifier)targetColorId, "BackBuffer color");
            //else if(m_TargetColorHandle.nameID != targetColorId)
            //    RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetColorHandle, targetColorId);
            
            RenderTargetIdentifier targetDepthId = BuiltinRenderTextureType.Depth;
            if(m_TargetDepthHandle == null)
                m_TargetDepthHandle = RTHandles.Alloc((RenderTargetIdentifier)targetDepthId, "BackBuffer depth");
            else if(m_TargetDepthHandle.nameID != targetDepthId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetDepthHandle, targetDepthId);

            Color clearColor = cameraData.GetClearColor();
            RTClearFlags clearFlags = cameraData.GetClearFlags();
            
            bool clearOnFirstUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardOnLastUse = !renderGraph.nativeRenderPassesEnabled;
            ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
            importBackbufferColorParams.clearOnFirstUse = clearOnFirstUse;
            importBackbufferColorParams.clearColor = clearColor;
            importBackbufferColorParams.discardOnLastUse = discardOnLastUse;
            
            ImportResourceParams importBackbufferDepthParams = new ImportResourceParams();
            importBackbufferDepthParams.clearOnFirstUse = clearOnFirstUse;
            importBackbufferDepthParams.clearColor = clearColor;
            importBackbufferDepthParams.discardOnLastUse = discardOnLastUse;
#if UNITY_EDITOR
            // on TBDR GPUs like Apple M1/M2, we need to preserve the backbuffer depth for overlay cameras in Editor for Gizmos
            if (cameraData.camera.cameraType == CameraType.SceneView)
                importBackbufferDepthParams.discardOnLastUse = false;
#endif
            
            bool colorRT_sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            RenderTargetInfo importInfoColor = new RenderTargetInfo();
            RenderTargetInfo importInfoDepth = new RenderTargetInfo();
            
            importInfoColor.width = Screen.width;
            importInfoColor.height = Screen.height;
            importInfoColor.volumeDepth = 1;
            importInfoColor.msaaSamples = 1;
            importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);
            importInfoColor.bindMS = false;
            
            importInfoDepth = importInfoColor;
            importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            
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