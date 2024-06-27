using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawTransparentObjectsProfilingSampler = new ProfilingSampler("DrawTransparentObjectsPass");
        internal class DrawTransparentObjectsPassData
        {
            internal RendererListHandle transparentRendererListHandle;
        }
        private void AddDrawTransparentObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawTransparentObjectsPassData>("Draw Transparent Objects Pass", out var passData, s_DrawTransparentObjectsProfilingSampler))
            {
                //创建半透明对象渲染列表
                RendererListDesc transparentRendererDesc = new RendererListDesc(s_shaderTagIds, cameraData.cullingResults, cameraData.camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transparentRendererListHandle = renderGraph.CreateRendererList(transparentRendererDesc);
                //RenderGraph引用不透明渲染列表
                builder.UseRendererList(passData.transparentRendererListHandle);

                if (m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                if (m_BackbufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackbufferDepthHandle, AccessFlags.Write);
                
                //设置主光源阴影
                if (m_MainLightShadowHandle.IsValid())
                    builder.UseTexture(m_MainLightShadowHandle, AccessFlags.Read);

                //设置渲染全局状态
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.SetRenderFunc((DrawTransparentObjectsPassData data, RasterGraphContext context)=> 
                {
                    context.cmd.SetGlobalFloat(ShaderPropertyId.alphaToMaskAvailable, 0.0f);
                    //调用渲染指令绘制
                    context.cmd.DrawRendererList(data.transparentRendererListHandle);
                });
            }
        }
    }
}