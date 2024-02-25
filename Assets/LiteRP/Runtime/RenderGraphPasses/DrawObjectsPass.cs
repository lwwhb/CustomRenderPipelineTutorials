using LiteRP.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;
namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); 
        internal class DrawObjectsPassData
        {
            internal TextureHandle colorHandle;
            internal TextureHandle depthHandle;
            
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
        }
        private void AddDrawObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passData, s_DrawObjectsProfilingSampler))
            {
                builder.UseAllGlobalTextures(true);
                
                if (m_BackBufferColorHandle.IsValid())
                {
                    passData.colorHandle = m_BackBufferColorHandle;
                    builder.SetRenderAttachment(m_BackBufferColorHandle, 0, AccessFlags.Write);
                }

                if (m_BackBufferDepthHandle.IsValid())
                {
                    passData.depthHandle = m_BackBufferDepthHandle;
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthHandle, AccessFlags.Write);
                }
                
                RendererListDesc opaqueRendererDesc = new RendererListDesc(s_shaderTagId, cameraData.cullingResults, cameraData.camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(opaqueRendererDesc);
                builder.UseRendererList(passData.opaqueRendererListHandle);
                
                RendererListDesc transparentRendererDesc = new RendererListDesc(s_shaderTagId, cameraData.cullingResults, cameraData.camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transparentRendererListHandle = renderGraph.CreateRendererList(transparentRendererDesc);
                builder.UseRendererList(passData.transparentRendererListHandle);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawObjectsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.opaqueRendererListHandle);
                    context.cmd.DrawRendererList(data.transparentRendererListHandle);
                });
            }
        }
    }
}