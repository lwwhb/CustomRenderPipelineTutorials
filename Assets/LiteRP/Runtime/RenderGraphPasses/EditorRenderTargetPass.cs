using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_EditorRenderTargetProfilingSampler = new ProfilingSampler("EditorRenderTargetPass");
        internal class EditorRenderTargetPassData
        {
        }
        private void AddEditorRenderTargetPass(RenderGraph renderGraph)
        {
            using (var builder =
                   renderGraph.AddUnsafePass<EditorRenderTargetPassData>("Editor RenderTarget Pass", out var passData,
                       s_EditorRenderTargetProfilingSampler))
            {
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((EditorRenderTargetPassData data, UnsafeGraphContext context) =>
                {
                    context.cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, // color
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare); // depth
                });
            }
        }
    }
}