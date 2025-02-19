using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawSkyBoxProfilingSampler = new ProfilingSampler("DrawSkyBoxPass");
        internal class SkyBoxPassData
        {
            internal RendererListHandle skyboxRenderListHandle;
        }
        private void AddDrawSkyBoxPass(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<SkyBoxPassData>("Draw SkyBox Pass", out var passData,
                       s_DrawSkyBoxProfilingSampler))
            {
                passData.skyboxRenderListHandle = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                builder.UseRendererList(passData.skyboxRenderListHandle);
                
                if(renderTargetData.activeColorTexture.IsValid())
                    builder.SetRenderAttachment(renderTargetData.activeColorTexture, 0, AccessFlags.Write);
                if (renderTargetData.activeDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(renderTargetData.activeDepthTexture, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((SkyBoxPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRenderListHandle);
                });
            }
        }
    }
}