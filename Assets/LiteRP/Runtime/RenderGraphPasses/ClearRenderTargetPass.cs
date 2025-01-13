using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_ClearRenderTargetProfilingSampler = new ProfilingSampler("ClearRenderTargetPass");
        internal class ClearRenderTargetPassData
        {
            internal RTClearFlags clearFlags;
            internal Color clearColor;
        }
        
        private void AddClearRenderTargetPass(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<ClearRenderTargetPassData>("Clear Render Target Pass",
                       out var passData, s_ClearRenderTargetProfilingSampler))
            {
                passData.clearFlags = cameraData.GetClearFlags();
                passData.clearColor = cameraData.GetClearColor();
               
                if(renderTargetData.backBufferColor.IsValid())
                    builder.SetRenderAttachment(renderTargetData.backBufferColor, 0, AccessFlags.Write);
                if (renderTargetData.backBufferDepth.IsValid())
                    builder.SetRenderAttachmentDepth(renderTargetData.backBufferDepth, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((ClearRenderTargetPassData data, RasterGraphContext context)=> 
                {
                    context.cmd.ClearRenderTarget(data.clearFlags, data.clearColor, 1, 0);
                });
            }
        }
    }
}