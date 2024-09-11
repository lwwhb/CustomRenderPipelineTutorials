using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        // 剔除结果
        public CullingResults cullingResults;
        // 最大阴影距离
        public float maxShadowDistance;
        //后处理是否开启
        public bool postProcessEnabled;

        public override void Reset()
        {
            camera = null;
            cullingResults = default;
            maxShadowDistance = 0.0f;
            postProcessEnabled = false;
        }

        public float GetCameraAspectRatio()
        {
            return (float)camera.pixelWidth / (float)camera.pixelHeight;
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