using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderer : IDisposable
    {
        private RTHandle m_TargetColorHandle = null;
        private RTHandle m_TargetDepthHandle = null;
        
        public LiteRenderer()
        {
            RenderTargetIdentifier targetColorId = BuiltinRenderTextureType.CameraTarget;
            RenderTargetIdentifier targetDepthId = BuiltinRenderTextureType.Depth;
            m_TargetColorHandle = RTHandles.Alloc(targetColorId, "Backbuffer color");
            m_TargetDepthHandle = RTHandles.Alloc(targetDepthId, "Backbuffer depth");
        }
        public void Dispose()
        {
            m_TargetColorHandle?.Release();
            m_TargetDepthHandle?.Release();
            
            GC.SuppressFinalize(this);
        }
        
        public void RecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, Camera camera)
        {
            AddClearBackbufferPass(renderGraph, camera);
            if(camera.clearFlags == CameraClearFlags.Skybox)
                AddDrawSkyBoxPass(renderGraph, context, camera);
        }
        private RTClearFlags GetCameraClearFlags(Camera camera)
        {
            var cameraClearFlags = camera.clearFlags;
            if (cameraClearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null /*&& cameraData.postProcessEnabled*/) //待完善
                return (RTClearFlags)ClearFlag.All;

            if ((cameraClearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) ||
                cameraClearFlags == CameraClearFlags.Nothing)
                return (RTClearFlags)ClearFlag.DepthStencil;

            return (RTClearFlags)ClearFlag.All;
        }
        
        private static readonly ProfilingSampler s_ClearBackbufferProfilingSampler = new ProfilingSampler("Clear BackBuffer");
        private static readonly ProfilingSampler s_DrawSkyBoxProfilingSampler = new ProfilingSampler("Draw SkyBox");
        
        internal class ClearBackbufferPassData
        {
            public TextureHandle color;
            public TextureHandle depth;

            public bool drawSkyBox;
            public bool clearDepth;
            public bool clearColor;
            public Color backgroundColor;

        }

        private ClearBackbufferPassData AddClearBackbufferPass(RenderGraph renderGraph, Camera camera)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<ClearBackbufferPassData>("ClearBackbufferPass", out var passData,
                       s_ClearBackbufferProfilingSampler))
            {
                RenderTargetInfo importInfoColor = new RenderTargetInfo();
                importInfoColor.width = Screen.width;
                importInfoColor.height = Screen.height;
                importInfoColor.volumeDepth = 1;
                importInfoColor.msaaSamples = 1;
                importInfoColor.format = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR); //临时代码

                RenderTargetInfo importInfoDepth = importInfoColor;
                importInfoDepth.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
    
                TextureHandle colorHandle = renderGraph.ImportTexture(m_TargetColorHandle, importInfoColor);
                TextureHandle depthHandle = renderGraph.ImportTexture(m_TargetDepthHandle, importInfoDepth);
                if (colorHandle.IsValid())
                {
                    passData.color = colorHandle;
                    builder.SetRenderAttachment(colorHandle, 0, AccessFlags.Write);
                }

                if (depthHandle.IsValid())
                {
                    passData.depth = depthHandle;
                    builder.SetRenderAttachmentDepth(depthHandle, AccessFlags.Write);
                }

                var clearFlags = camera.clearFlags;
                passData.drawSkyBox = clearFlags == CameraClearFlags.Skybox? true : false;
                passData.clearDepth = clearFlags != CameraClearFlags.Nothing;
                passData.clearColor = clearFlags == CameraClearFlags.Color? true : false;
                passData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);
                
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((ClearBackbufferPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(passData.clearDepth, passData.clearColor,passData.backgroundColor);
                });
                return passData;
            }
        }

        
        internal class SkyBoxPassData
        {
            public RendererList skyboxRenderList;
        }
        private void AddDrawSkyBoxPass(RenderGraph renderGraph, ScriptableRenderContext context, Camera camera)
        {
            using (var builder =
                   renderGraph.AddRenderPass<SkyBoxPassData>("DrawSkyBoxPass", out var passData,
                       s_DrawSkyBoxProfilingSampler))
            {
                passData.skyboxRenderList = context.CreateSkyboxRendererList(camera);
                builder.SetRenderFunc((SkyBoxPassData data, RenderGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRenderList);
                });
            }
        }
    }
}