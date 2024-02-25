using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        public CullingResults cullingResults;
        public override void Reset()
        {
            camera = null;
            cullingResults = default;
        }
        public ClearFlag GetCameraClearFlag()
        {
            var cameraClearFlags = camera.clearFlags;
            if (cameraClearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) 
                return ClearFlag.All;

            if ((cameraClearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) ||
                cameraClearFlags == CameraClearFlags.Nothing)
                return ClearFlag.DepthStencil;

            return ClearFlag.All;
        }

        public Color GetClearColor()
        {
            return CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);
        }
    }
}