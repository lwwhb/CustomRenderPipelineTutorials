using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_ClearTargetsProfilingSampler = new ProfilingSampler("Clear Targets");
        
        internal class ClearTargetsPassData
        {
            internal TextureHandle color;
            internal TextureHandle depth;

            internal RTClearFlags clearFlags;
            internal Color clearColor;
        }

        private void AddClearTargetsPass(RenderGraph renderGraph, RTClearFlags clearFlags, Color clearColor)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<ClearTargetsPassData>("ClearTargetsPass", out var passData,
                       s_ClearTargetsProfilingSampler))
            {
                if (m_BackBufferColorHandle.IsValid())
                {
                    passData.color = m_BackBufferColorHandle;
                    builder.SetRenderAttachment(m_BackBufferColorHandle, 0, AccessFlags.Write);
                }

                if (m_BackBufferDepthHandle.IsValid())
                {
                    passData.depth = m_BackBufferDepthHandle;
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthHandle, AccessFlags.Write);
                }
                
                passData.clearFlags = clearFlags;
                passData.clearColor = clearColor;
                
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((ClearTargetsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(passData.clearFlags, passData.clearColor, 1, 0);
                });
            }
        }
    }
}