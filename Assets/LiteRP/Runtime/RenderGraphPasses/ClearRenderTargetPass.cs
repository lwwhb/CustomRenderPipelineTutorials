using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_ClearRenderTargetProfilingSampler = new ProfilingSampler("ClearRenderTargetPass");
        internal class ClearRenderTargetPassData
        {
            internal RTClearFlags clearFlags;
            internal Color clearColor;
        }
        
        private void AddClearRenderTargetPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<ClearRenderTargetPassData>("Clear Render Target Pass",
                       out var passData, s_ClearRenderTargetProfilingSampler))
            {
                passData.clearFlags = cameraData.GetClearFlags();
                passData.clearColor = cameraData.GetClearColor();
               
                if(m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                if (m_BackbufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackbufferDepthHandle, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((ClearRenderTargetPassData data, RasterGraphContext context)=> 
                {
                    context.cmd.ClearRenderTarget(data.clearFlags, data.clearColor, 1, 0);
                });
            }
        }
    }
}