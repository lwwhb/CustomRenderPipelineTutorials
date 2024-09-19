using LiteRP.FrameData;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_DrawEditorGizmoProfilingSampler = new ProfilingSampler("DrawEditorGizmoPass");
        internal class DrawEditorGizmoPassData
        {
            internal RendererListHandle gizmoRendererListHandle;
        }

        private void AddDrawEditorGizmoPass(RenderGraph renderGraph, CameraData cameraData, GizmoSubset gizmoSubset)
        {
#if UNITY_EDITOR
            if(!Handles.ShouldRenderGizmos() || cameraData.camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered)
                return;
            
            bool renderPreGizmos = (gizmoSubset == GizmoSubset.PreImageEffects);
            var passName = renderPreGizmos ? "Draw Pre Gizmos Pass" : "Draw Post Gizmos Pass";
            using (var builder = renderGraph.AddRasterRenderPass<DrawEditorGizmoPassData>(passName, out var passData,
                       s_DrawEditorGizmoProfilingSampler))
            {
                if (m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                if (m_BackbufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackbufferDepthHandle, AccessFlags.Read);

                passData.gizmoRendererListHandle = renderGraph.CreateGizmoRendererList(cameraData.camera, gizmoSubset);
                builder.UseRendererList(passData.gizmoRendererListHandle);
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawEditorGizmoPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.gizmoRendererListHandle);
                });
            }
#endif
        }
    }
}