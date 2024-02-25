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
            internal RendererList skyboxRenderList;
        }
        private void AddDrawSkyBoxPass(RenderGraph renderGraph, RenderData renderData, CameraData cameraData)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<SkyBoxPassData>("DrawSkyBoxPass", out var passData,
                       s_DrawSkyBoxProfilingSampler))
            {
                if(m_BackBufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorHandle, 0, AccessFlags.Write);
                if(m_BackBufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthHandle, AccessFlags.Write);
                
                passData.skyboxRenderList = renderData.renderContext.CreateSkyboxRendererList(cameraData.camera);
                builder.SetRenderFunc((SkyBoxPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRenderList);
                });
            }
        }
    }
}