using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        public float renderScale = 1.0f; //lwwhb临时，之后放到asset中去
        public int scaledWidth => Mathf.Max(1, (int)(camera.pixelWidth * renderScale));
        public int scaledHeight => Mathf.Max(1, (int)(camera.pixelHeight * renderScale));
        // 剔除结果
        public CullingResults cullingResults;
        // 最大阴影距离
        public float maxShadowDistance;
        // 用于创建Inmmidiate Camera RT的描述符
        public RenderTextureDescriptor cameraTargetDescriptor;
        //后处理是否开启
        public bool postProcessEnabled;
        // HDR是否开启
        public bool isHdrEnabled;
        // HDR颜色缓冲区精度
        internal HDRColorBufferPrecision hdrColorBufferPrecision;

        public override void Reset()
        {
            camera = null;
            renderScale = 1.0f;
            cullingResults = default;
            maxShadowDistance = 0.0f;
            cameraTargetDescriptor = default;
            postProcessEnabled = false;
            isHdrEnabled = false;
            hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
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
        
        public bool IsHandleYFlipped(RTHandle handle)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
                return true;

            if (camera.cameraType == CameraType.SceneView || camera.cameraType == CameraType.Preview)
                return true;

            var handleID = new RenderTargetIdentifier(handle.nameID, 0, CubemapFace.Unknown, 0);
            bool isBackbuffer = handleID == BuiltinRenderTextureType.CameraTarget || handleID == BuiltinRenderTextureType.Depth;
            return !isBackbuffer;
        }
        
        public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
                return true;

            return camera.targetTexture != null || IsHandleYFlipped(color ?? depth);
        }
    }
}