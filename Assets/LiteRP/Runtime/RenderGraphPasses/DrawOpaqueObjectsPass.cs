using LiteRP.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawOpaqueObjectsProfilingSampler = new ProfilingSampler("DrawOpaqueObjectsPass");
        internal class DrawOpaqueObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
        }
        private void AddDrawOpaqueObjectsPass(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawOpaqueObjectsPassData>("Draw Opaque Objects Pass", out var passData, s_DrawOpaqueObjectsProfilingSampler))
            {
                //创建不透明对象渲染列表
                RendererListDesc opaqueRendererDesc = new RendererListDesc(s_shaderTagIds, cameraData.cullingResults, cameraData.camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(opaqueRendererDesc);
                //RenderGraph引用不透明渲染列表
                builder.UseRendererList(passData.opaqueRendererListHandle);

                if (renderTargetData.backBufferColor.IsValid())
                    builder.SetRenderAttachment(renderTargetData.backBufferColor, 0, AccessFlags.Write);
                if (renderTargetData.backBufferDepth.IsValid())
                    builder.SetRenderAttachmentDepth(renderTargetData.backBufferDepth, AccessFlags.Write);
                
                //设置主光源阴影
                if (renderTargetData.mainLightShadow.IsValid())
                    builder.UseTexture(renderTargetData.mainLightShadow, AccessFlags.Read);

                //设置渲染全局状态
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.SetRenderFunc((DrawOpaqueObjectsPassData data, RasterGraphContext context)=> 
                {
                    context.cmd.SetGlobalFloat(ShaderPropertyId.alphaToMaskAvailable, 1.0f);
                    //调用渲染指令绘制
                    context.cmd.DrawRendererList(data.opaqueRendererListHandle);
                });
            }
        }
    }
}   