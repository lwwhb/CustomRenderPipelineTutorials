using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawSkyBoxProfilingSampler = new ProfilingSampler("Draw SkyBox");
        internal class SkyBoxPassData
        {
            internal RendererListHandle skyboxRenderListHandle;
        }
        private void AddDrawSkyBoxPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<SkyBoxPassData>("DrawSkyBoxPass", out var passData,
                       s_DrawSkyBoxProfilingSampler))
            {
                if(m_BackBufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorHandle, 0, AccessFlags.Write);
                if(m_BackBufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthHandle, AccessFlags.Write);
                
                passData.skyboxRenderListHandle = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                builder.UseRendererList(passData.skyboxRenderListHandle);
                
                builder.SetRenderFunc((SkyBoxPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRenderListHandle);
                });
            }
        }
    }
}