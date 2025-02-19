using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_FinalBlitProfilingSampler = new ProfilingSampler("FinalBlitPass");
        
        static class BlitPassNames
        {
            public const string NearestSampler = "NearestDebugDraw";
            public const string BilinearSampler = "BilinearDebugDraw";
        }
        enum BlitType
        {
            Core = 0, // Core blit
            HDR = 1, // Blit with HDR encoding and overlay UI compositing
            Count = 2
        }

        internal struct BlitMaterialData
        {
            public Material material;
            public int nearestSamplerPass;
            public int bilinearSamplerPass;
        }

        BlitMaterialData[] m_BlitMaterialData;
        internal class FinalPassData
        {
            internal TextureHandle sourceColor;
            internal TextureHandle destTarget;
            internal bool enableAlphaOutput;
            internal BlitMaterialData blitMaterialData;
            internal CameraData cameraData;
        }
        
        private bool NeedFinalBlitPass()
        {
            return m_RequiresIntermediateAttachments;
        }

        private void AddFinalBlitPass(RenderGraph renderGraph, RenderTargetData renderTargetData, CameraData cameraData)
        {
            using (var builder =
                   renderGraph.AddRasterRenderPass<FinalPassData>("Final Blit Pass", out var passData,
                       s_FinalBlitProfilingSampler))
            {
                // Find sampler passes by name
                const int blitTypeCount = (int)BlitType.Count;
                m_BlitMaterialData = new BlitMaterialData[blitTypeCount];
                for (int i = 0; i < blitTypeCount; ++i)
                {
                    m_BlitMaterialData[i].material = i == (int)BlitType.Core ? m_BlitMaterial : m_BlitHDRMaterial;
                    m_BlitMaterialData[i].nearestSamplerPass = m_BlitMaterialData[i].material?.FindPass(BlitPassNames.NearestSampler) ?? -1;
                    m_BlitMaterialData[i].bilinearSamplerPass = m_BlitMaterialData[i].material?.FindPass(BlitPassNames.BilinearSampler) ?? -1;
                }
                
                passData.sourceColor = renderTargetData.frontBufferColor;
                builder.UseTexture(renderTargetData.frontBufferColor, AccessFlags.Read);
                passData.destTarget = renderTargetData.backBufferColor;
                passData.enableAlphaOutput = GraphicsFormatUtility.HasAlphaChannel(cameraData.cameraTargetDescriptor.graphicsFormat);
                passData.blitMaterialData = m_BlitMaterialData[(int)BlitType.Core];     // lwwhb 暂时不支持HDR Output
                passData.cameraData = cameraData;
                builder.SetRenderAttachment(renderTargetData.backBufferColor, 0, AccessFlags.Write);

                //设置渲染全局状态
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.SetRenderFunc((FinalPassData data, RasterGraphContext context) =>
                {
                    bool isRenderToBackBufferTarget = cameraData.camera.cameraType != CameraType.SceneView;
                    Vector4 scaleBias = RenderingUtils.GetFinalBlitScaleBias(data.sourceColor, data.destTarget, cameraData);
                    if (isRenderToBackBufferTarget)
                        context.cmd.SetViewport(cameraData.camera.pixelRect);

                    // turn off any global wireframe & "scene view wireframe shader hijack" settings for doing blits:
                    // we never want them to show up as wireframe
                    context.cmd.SetWireframe(false);

                    CoreUtils.SetKeyword(data.blitMaterialData.material, ShaderKeywordStrings.EnableAlphaOutput, data.enableAlphaOutput);
                    RTHandle source = data.sourceColor;
                    int shaderPassIndex = source.rt?.filterMode == FilterMode.Bilinear ? data.blitMaterialData.bilinearSamplerPass : data.blitMaterialData.nearestSamplerPass;
                    Blitter.BlitTexture(context.cmd, data.sourceColor, scaleBias, data.blitMaterialData.material, shaderPassIndex);
                });
            }
        }
    }
}
