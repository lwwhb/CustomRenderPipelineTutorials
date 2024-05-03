using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_SetupCameraPropertiesProfilingSampler = new ProfilingSampler("SetupCameraPropertiesPass");
        internal class SetupCameraPropertiesPassData
        {
            internal CameraData cameraData;
        }

        private void AddSetupCameraPropertiesPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SetupCameraPropertiesPassData>("Setup Camera Properties Pass", out var passData,
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