using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
#if UNITY_EDITOR
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_SetEditorTargetProfilingSampler = new ProfilingSampler("Set Editor Target");
        private class SetEditorTargetPassData
        {
        }

        private void AddSetEditorTargetPass(RenderGraph renderGraph)
        {
            using (var builder = renderGraph.AddUnsafePass<SetEditorTargetPassData>("SetEditorTargetPass", out var passData,
                       s_SetEditorTargetProfilingSampler))
            {
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((SetEditorTargetPassData data, UnsafeGraphContext context) =>
                {
                    context.cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, // color
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare); // depth
                });
            }
        }
    }
#endif
}