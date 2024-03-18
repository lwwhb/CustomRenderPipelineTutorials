using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class LiteRPUtils
    {
        public static bool CanNativeRenderPassesEnabled()
        {
            return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12
                   && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 // GLES doesn't support backbuffer MSAA resolve with the NRP API
                   && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore;
        }
    }
}