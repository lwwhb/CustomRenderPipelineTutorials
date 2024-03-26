using LiteRP.FrameData;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_DrawEditorGizmoProfilingSampler = new ProfilingSampler("DrawEditorGizmoPass");
        internal class DrawEditorGizmoPassData
        {
            internal RendererListHandle gizmoRendererListHandle;
        }

        private void AddDrawEditorGizmoPass(RenderGraph renderGraph, CameraData cameraData, GizmoSubset gizmoSubset)
        {
            if(!Handles.ShouldRenderGizmos() || cameraData.camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered)
                return;
            
            using (var builder = renderGraph.AddRasterRenderPass<DrawEditorGizmoPassData>("Draw Editor Gizmo Pass", out var passData,
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
        }
    }
}