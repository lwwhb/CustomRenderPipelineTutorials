using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace LiteRP
{
    // HDR颜色缓冲区精度定义
    public enum HDRColorBufferPrecision
    {
        /// <summary> Typically R11G11B10f for faster rendering. Recommend for mobile.
        /// R11G11B10f can cause a subtle blue/yellow banding in some rare cases due to lower precision of the blue component.</summary>
        [Tooltip("Use 32-bits per pixel for HDR rendering.")]
        _32Bits,
        /// <summary>Typically R16G16B16A16f for better quality. Can reduce banding at the cost of memory and performance.</summary>
        [Tooltip("Use 64-bits per pixel for HDR rendering.")]
        _64Bits,
    }
    
    // 对NativeArrary的Unsafe扩展
    static class NativeArrayExtensions
    {
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        public static unsafe ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
    }
    
    public static class LiteRPUtils
    {
        //全局获取管线Asset
        internal static LiteRPAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as LiteRPAsset;
        }
        //创建渲染纹理描述
        internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, int msaaSamples)
        {
            RenderTextureDescriptor desc;
            if (camera.targetTexture == null)
            {
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
                desc.width = camera.pixelWidth;
                desc.height = camera.pixelHeight;
                desc.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
                desc.depthBufferBits = 32;
                desc.msaaSamples = msaaSamples;
                desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            }
            else
            {
                desc = camera.targetTexture.descriptor;
                desc.msaaSamples = msaaSamples;
                desc.width = camera.pixelWidth;
                desc.height = camera.pixelHeight;

                if (camera.cameraType == CameraType.SceneView)
                {
                    desc.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
                }
            }
            desc.width = Mathf.Max(1, desc.width);
            desc.height = Mathf.Max(1, desc.height);

            desc.enableRandomWrite = false;
            desc.bindMS = false;
            desc.useDynamicScale = camera.allowDynamicResolution;
            desc.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(desc);
            if (!SystemInfo.supportsStoreAndResolveAction)
                desc.msaaSamples = 1;

            return desc;
        }
        
        internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, float renderScale,
            bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, int msaaSamples, bool needsAlpha)
        {
            int scaledWidth = (int)((float)camera.pixelWidth * renderScale);
            int scaledHeight = (int)((float)camera.pixelHeight * renderScale);

            RenderTextureDescriptor desc;

            if (camera.targetTexture == null)
            {
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
                desc.width = scaledWidth;
                desc.height = scaledHeight;
                desc.graphicsFormat = MakeRenderTextureGraphicsFormat(isHdrEnabled, requestHDRColorBufferPrecision, needsAlpha);
                desc.depthBufferBits = 32;
                desc.msaaSamples = msaaSamples;
                desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            }
            else
            {
                desc = camera.targetTexture.descriptor;
                desc.msaaSamples = msaaSamples;
                desc.width = scaledWidth;
                desc.height = scaledHeight;

                if (camera.cameraType == CameraType.SceneView && !isHdrEnabled)
                {
                    desc.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
                }
            }
            desc.width = Mathf.Max(1, desc.width);
            desc.height = Mathf.Max(1, desc.height);

            desc.enableRandomWrite = false;
            desc.bindMS = false;
            desc.useDynamicScale = camera.allowDynamicResolution;
            desc.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(desc);
            if (!SystemInfo.supportsStoreAndResolveAction)
                desc.msaaSamples = 1;

            return desc;
        }
        
        internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, bool needsAlpha)
        {
            if (isHdrEnabled)
            {
                // TODO: we need a proper format scoring system. Score formats, sort, pick first or pick first supported (if not in score).
                // UUM-41070: We require `Linear | Render` but with the deprecated FormatUsage this was checking `Blend`
                // For now, we keep checking for `Blend` until the performance hit of doing the correct checks is evaluated
                if (!needsAlpha && requestHDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
                    return GraphicsFormat.B10G11R11_UFloatPack32;
                if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
                    return GraphicsFormat.R16G16B16A16_SFloat;
                return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR); // This might actually be a LDR format on old devices.
            }

            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
        }

        // Returns a UNORM based render texture format
        // When supported by the device, this function will prefer formats with higher precision, but the same bit-depth
        // NOTE: This function does not guarantee that the returned format will contain an alpha channel.
        internal static GraphicsFormat MakeUnormRenderTextureGraphicsFormat()
        {
            // UUM-41070: We require `Linear | Render` but with the deprecated FormatUsage this was checking `Blend`
            // For now, we keep checking for `Blend` until the performance hit of doing the correct checks is evaluated
            if (SystemInfo.IsFormatSupported(GraphicsFormat.A2B10G10R10_UNormPack32, GraphicsFormatUsage.Blend))
                return GraphicsFormat.A2B10G10R10_UNormPack32;
            else
                return GraphicsFormat.R8G8B8A8_UNorm;
        }
    }
}