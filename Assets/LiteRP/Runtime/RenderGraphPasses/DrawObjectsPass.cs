using LiteRP.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); //渲染标签ID
        internal class DrawObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
        }
        private void AddDrawObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passData, s_DrawObjectsProfilingSampler))
            {
                //创建不透明对象渲染列表
                RendererListDesc opaqueRendererDesc = new RendererListDesc(s_shaderTagId, cameraData.cullingResults, cameraData.camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(opaqueRendererDesc);
                //RenderGraph引用不透明渲染列表
                builder.UseRendererList(passData.opaqueRendererListHandle);
                
                //创建半透明对象渲染列表
                RendererListDesc transparentRendererDesc = new RendererListDesc(s_shaderTagId, cameraData.cullingResults, cameraData.camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transparentRendererListHandle = renderGraph.CreateRendererList(transparentRendererDesc);
                //RenderGraph引用不透明渲染列表
                builder.UseRendererList(passData.transparentRendererListHandle);
                
                builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                
                //设置渲染全局状态
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context)=> 
                {
                    //调用渲染指令绘制
                    context.cmd.DrawRendererList(passData.opaqueRendererListHandle);
                    context.cmd.DrawRendererList(passData.transparentRendererListHandle);
                });
            }
        }
    }
}   