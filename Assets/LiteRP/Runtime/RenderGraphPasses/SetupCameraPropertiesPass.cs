using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_SetupCameraPropertiesProfilingSampler = new ProfilingSampler("Setup Camera Properties");
        internal class SetupCameraPropertiesPassData
        {
            internal CameraData cameraData;
        }

        private void AddSetupCameraPropertiesPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SetupCameraPropertiesPassData>("SetupCameraProperties", out var passData,
                       s_SetupCameraPropertiesProfilingSampler))
            {
                passData.cameraData = cameraData;
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc((SetupCameraPropertiesPassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetupCameraProperties(data.cameraData.camera);
                });
            }
        }
    }
}