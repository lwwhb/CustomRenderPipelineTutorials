using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        internal class DrawObjectsPassData
        {}
        private void AddDrawObjectsPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passData, s_DrawObjectsProfilingSampler))
            {
                //声明创建或引用的资源
                
                //设置渲染全局状态
                
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context)=> 
                {
                    //调用渲染指令绘制
                });
            }
        }
    }
}   