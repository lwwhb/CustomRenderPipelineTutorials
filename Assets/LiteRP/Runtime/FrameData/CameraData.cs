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

        public RTClearFlags GetClearFlags()
        {
            CameraClearFlags clearFlags = camera.clearFlags;
            if(clearFlags == CameraClearFlags.Depth)
            {
                return RTClearFlags.DepthStencil;
            }
            else if(clearFlags == CameraClearFlags.Nothing)
            {
                return RTClearFlags.None;
            }
            return RTClearFlags.All;
        }

        public Color GetClearColor()
        {
            return CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);
        }
    }
}