using System;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder : IDisposable, IRenderGraphRecorder
    {
        private TextureHandle m_BackBufferColorHandle = TextureHandle.nullHandle;
        private TextureHandle m_BackBufferDepthHandle = TextureHandle.nullHandle;
        private RTHandle m_TargetColorHandle = null;
        private RTHandle m_TargetDepthHandle = null;
        
        public LiteRenderGraphRecorder()
        {
        }
        public void Dispose()
        {
            m_TargetColorHandle?.Release();
            m_TargetDepthHandle?.Release();
            
            GC.SuppressFinalize(this);
        }
        
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<CameraData>();
            CreateRenderGraphCameraRenderTargets(renderGraph, frameData);
                
            //设置相机参数
            AddSetupCameraPropertiesPass(renderGraph, cameraData);
            
            if (!renderGraph.nativeRenderPassesEnabled)
            {
                RTClearFlags clearFlags = (RTClearFlags) cameraData.GetCameraClearFlag();
                if (clearFlags != RTClearFlags.None)
                    AddClearTargetsPass(renderGraph, clearFlags, cameraData.GetClearColor());
            }
            if(cameraData.camera.clearFlags == CameraClearFlags.Skybox)
                AddDrawSkyBoxPass(renderGraph, cameraData);
            
            AddDrawObjectsPass(renderGraph, cameraData);
#if UNITY_EDITOR
            if (cameraData.camera.cameraType == CameraType.SceneView)
                AddSetEditorTargetPass(renderGraph);
#endif
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<CameraData>();
            var cameraTargetTexture = cameraData.camera.targetTexture;
            bool isBuiltInTexture = (cameraTargetTexture == null);
            
            RenderTargetIdentifier targetColorId = isBuiltInTexture? BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier(cameraTargetTexture);
            RenderTargetIdentifier targetDepthId = isBuiltInTexture ? BuiltinRenderTextureType.Depth : new RenderTargetIdentifier(cameraTargetTexture);
            if(m_TargetColorHandle == null)
                m_TargetColorHandle = RTHandles.Alloc(targetColorId, "Backbuffer color");
            else if(m_TargetColorHandle.nameID != targetColorId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetColorHandle, targetColorId);
            if(m_TargetDepthHandle == null)
                m_TargetDepthHandle = RTHandles.Alloc(targetDepthId, "Backbuffer depth");
            else if (m_TargetDepthHandle.nameID != targetDepthId)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_TargetDepthHandle, targetDepthId);
            
            
            Color cameraBackgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(cameraData.camera.backgroundColor);
            
            ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
            importBackbufferColorParams.clearOnFirstUse = false;
            importBackbufferColorParams.clearColor = cameraBackgroundColor;
            importBackbufferColorParams.discardOnLastUse = false;

            ImportResourceParams importBackbufferDepthParams = new ImportResourceParams();
            importBackbufferDepthParams.clearOnFirstUse = false;
            importBackbufferDepthParams.clearColor = cameraBackgroundColor;
            importBackbufferDepthParams.discardOnLastUse = false;
            
            bool colorRT_sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            RenderTargetInfo importInfoColor = new RenderTargetInfo();
            RenderTargetInfo importInfoDepth = new RenderTargetInfo();
            if (isBuiltInTexture)
            {
                importInfoColor.width = Screen.width;
                importInfoColor.height = Screen.height;
                importInfoColor.volumeDepth = 1;
                importInfoColor.msaaSamples = 1;
                importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);

                importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            }
            else
            {
                Debug.Log("width:" + cameraTargetTexture.width + " height:" + cameraTargetTexture.height);
                importInfoColor.width = cameraTargetTexture.width;
                importInfoColor.height = cameraTargetTexture.height;
                importInfoColor.volumeDepth = cameraTargetTexture.volumeDepth;
                importInfoColor.msaaSamples = cameraTargetTexture.antiAliasing;
                importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);

                importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
            }

            m_BackBufferColorHandle = renderGraph.ImportTexture(m_TargetColorHandle, importInfoColor, importBackbufferColorParams);
            m_BackBufferDepthHandle = renderGraph.ImportTexture(m_TargetDepthHandle, importInfoDepth, importBackbufferDepthParams);
        }
    }
}