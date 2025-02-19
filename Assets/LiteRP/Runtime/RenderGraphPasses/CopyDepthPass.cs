using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_CopyDepthProfilingSampler = new ProfilingSampler("CopyDepthPass");
        private RenderTextureDescriptor m_CameraDepthTextureDescriptor;
        internal class CopyDepthPassData
        {
            internal RTHandle sourceDepth;
            internal Material copyDepthMaterial;
            internal CameraData cameraData;
        }

        private bool NeedCopyDepthPass(CameraData cameraData)
        {
            return false;
        }
        private void UpdateCameraDepthTextureDescriptorIfNeeded(RenderGraph renderGraph, CameraData cameraData)
        {
            if ( m_CameraDepthTextureDescriptor.width != cameraData.camera.scaledPixelWidth
                 || m_CameraDepthTextureDescriptor.height != cameraData.camera.scaledPixelHeight)
            {
                m_CameraDepthTextureDescriptor = new RenderTextureDescriptor(cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, GraphicsFormat.D32_SFloat, GraphicsFormat.D32_SFloat);
            }
        }
        private void AddCopyDepthPass(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            /*using (var builder =
                   renderGraph.AddRasterRenderPass<CopyDepthPassData>("Copy Depth Pass", out var passData,
                       s_CopyDepthProfilingSampler))
            {
                passData.copyDepthMaterial = m_CopyDepthMaterial;
                passData.cameraData = cameraData;
                if (renderTargetData.cameraDepth.IsValid())
                {
                    UpdateCameraDepthTextureDescriptorIfNeeded(renderGraph, cameraData);
                    passData.sourceDepth = m_CameraDepthAttachment;
                    builder.UseTexture(renderTargetData.cameraDepth, AccessFlags.Read);
                }
                
                renderTargetData.cameraDepth = LiteRPRenderGraphUtils.CreateRenderGraphTexture(renderGraph,
                    m_CameraDepthTextureDescriptor, ShaderPropertyName.cameraDepthTextureName, true, Color.black);
                if (renderTargetData.cameraDepth.IsValid())
                {
                    builder.SetRenderAttachmentDepth(renderTargetData.cameraDepth, AccessFlags.WriteAll);
                    builder.SetGlobalTextureAfterPass(renderTargetData.cameraDepth,
                        ShaderPropertyId.cameraDepthTexture);
                }
                
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.SetRenderFunc((CopyDepthPassData data, RasterGraphContext context) =>
                {
                    if (passData.copyDepthMaterial == null)
                   {
                       Debug.LogErrorFormat("Missing {0}. Copy Depth render pass will not execute. Check for missing reference in the renderer resources.", copyDepthMaterial);
                       return;
                   }
               
                    bool yflip = passData.cameraData.IsHandleYFlipped(passData.sourceDepth);
                    Vector2 viewportScale = passData.sourceDepth.useScaling ? new Vector2(passData.sourceDepth.rtHandleProperties.rtHandleScale.x, passData.sourceDepth.rtHandleProperties.rtHandleScale.y) : Vector2.one;
                    Vector4 scaleBias = yflip ? new Vector4(viewportScale.x, -viewportScale.y, 0, viewportScale.y) : new Vector4(viewportScale.x, viewportScale.y, 0, 0);

                    passData.copyDepthMaterial.SetTexture(ShaderPropertyId.cameraDepthTexture, passData.sourceDepth);
                    Blitter.BlitTexture(context.cmd, passData.sourceDepth, scaleBias, passData.copyDepthMaterial, 0);
                });
            }*/
        }
    }
}
