using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public static class RenderingUtils
    {
        // Caches render texture format support. SystemInfo.SupportsRenderTextureFormat allocates memory due to boxing.
        static Dictionary<RenderTextureFormat, bool> m_RenderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();
        
        static Material s_ErrorMaterial;
        static Material errorMaterial
        {
            get
            {
                if (s_ErrorMaterial == null)
                {
                    // TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
                    // This might be in a point that some resources required for the pipeline are not finished importing yet.
                    // Proper fix is to add a fence on asset import.
                    try
                    {
                        s_ErrorMaterial = new Material(Shader.Find("Hidden/LiteRP/FallbackError"));
                    }
                    catch { }
                }

                return s_ErrorMaterial;
            }
        }
        
        internal static void ClearSystemInfoCache()
        {
            m_RenderTextureFormatSupport.Clear();
        }
        
        //运行时检测是否支持RT格式
        public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
        {
            if (!m_RenderTextureFormatSupport.TryGetValue(format, out var support))
            {
                support = SystemInfo.SupportsRenderTextureFormat(format);
                m_RenderTextureFormatSupport.Add(format, support);
            }

            return support;
        }
        
        /// <summary>
        /// Returns the scale bias vector to use for final blits to the backbuffer, based on scaling mode and y-flip platform requirements.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="cameraData"></param>
        /// <returns></returns>
        internal static Vector4 GetFinalBlitScaleBias(RTHandle source, RTHandle destination, CameraData cameraData)
        {
            Vector2 viewportScale = source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one;
            var yflip = cameraData.IsRenderTargetProjectionMatrixFlipped(destination);
            Vector4 scaleBias = !yflip ? new Vector4(viewportScale.x, -viewportScale.y, 0, viewportScale.y) : new Vector4(viewportScale.x, viewportScale.y, 0, 0);

            return scaleBias;
        }
        
        /// <summary>
        /// Re-allocate fixed-size RTHandle if it is not allocated or doesn't match the descriptor
        /// </summary>
        /// <param name="handle">RTHandle to check (can be null)</param>
        /// <param name="descriptor">Descriptor for the RTHandle to match</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <returns>If an allocation was done.</returns>
        public static bool ReAllocateHandleIfNeeded(
            ref RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
        {
            Assert.IsTrue(descriptor.graphicsFormat == GraphicsFormat.None ^ descriptor.depthStencilFormat == GraphicsFormat.None);

            TextureDesc requestRTDesc = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Explicit, anisoLevel, 0, filterMode, wrapMode, name);
            if (RTHandleNeedsReAlloc(handle, requestRTDesc, false))
            {
                var allocInfo = CreateRTHandleAllocInfo(descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name);
                handle = RTHandles.Alloc(descriptor.width, descriptor.height, allocInfo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Re-allocate dynamically resized RTHandle if it is not allocated or doesn't match the descriptor
        /// </summary>
        /// <param name="handle">RTHandle to check (can be null)</param>
        /// <param name="scaleFactor">Constant scale for the RTHandle size computation.</param>
        /// <param name="descriptor">Descriptor for the RTHandle to match</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <returns>If an allocation was done.</returns>
        public static bool ReAllocateHandleIfNeeded(
            ref RTHandle handle,
            Vector2 scaleFactor,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
        {
            var usingConstantScale = handle != null && handle.useScaling && handle.scaleFactor == scaleFactor;
            TextureDesc requestRTDesc = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Scale, anisoLevel, 0, filterMode, wrapMode);
            if (!usingConstantScale || RTHandleNeedsReAlloc(handle, requestRTDesc, true))
            {
                var allocInfo = CreateRTHandleAllocInfo(descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name);
                handle = RTHandles.Alloc(scaleFactor, allocInfo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Re-allocate dynamically resized RTHandle if it is not allocated or doesn't match the descriptor
        /// </summary>
        /// <param name="handle">RTHandle to check (can be null)</param>
        /// <param name="scaleFunc">Function used for the RTHandle size computation.</param>
        /// <param name="descriptor">Descriptor for the RTHandle to match</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <returns>If an allocation was done.</returns>
        public static bool ReAllocateHandleIfNeeded(
            ref RTHandle handle,
            ScaleFunc scaleFunc,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
        {
            var usingScaleFunction = handle != null && handle.useScaling && handle.scaleFactor == Vector2.zero;
            TextureDesc requestRTDesc = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Functor, anisoLevel, 0, filterMode, wrapMode);
            if (!usingScaleFunction || RTHandleNeedsReAlloc(handle, requestRTDesc, true))
            {
                var allocInfo = CreateRTHandleAllocInfo(descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name);
                handle = RTHandles.Alloc(scaleFunc, allocInfo);
                return true;
            }
            return false;
        }
        
        internal static bool RTHandleNeedsReAlloc(
            RTHandle handle,
            in TextureDesc descriptor,
            bool scaled)
        {
            if (handle == null || handle.rt == null)
                return true;
            if (handle.useScaling != scaled)
                return true;
            if (!scaled && (handle.rt.width != descriptor.width || handle.rt.height != descriptor.height))
                return true;
            if (handle.rt.enableShadingRate && handle.rt.graphicsFormat != descriptor.colorFormat)
                return true;

            var rtHandleFormat = (handle.rt.descriptor.depthStencilFormat != GraphicsFormat.None) ? handle.rt.descriptor.depthStencilFormat : handle.rt.descriptor.graphicsFormat;
            var isShadowMap = handle.rt.descriptor.shadowSamplingMode != ShadowSamplingMode.None;

            return
                rtHandleFormat != descriptor.format ||
                handle.rt.descriptor.dimension != descriptor.dimension ||
                handle.rt.descriptor.volumeDepth != descriptor.slices ||
                handle.rt.descriptor.enableRandomWrite != descriptor.enableRandomWrite ||
                handle.rt.descriptor.enableShadingRate != descriptor.enableShadingRate ||
                handle.rt.descriptor.useMipMap != descriptor.useMipMap ||
                handle.rt.descriptor.autoGenerateMips != descriptor.autoGenerateMips ||
                isShadowMap != descriptor.isShadowMap ||
                (MSAASamples)handle.rt.descriptor.msaaSamples != descriptor.msaaSamples ||
                handle.rt.descriptor.bindMS != descriptor.bindTextureMS ||
                handle.rt.descriptor.useDynamicScale != descriptor.useDynamicScale ||
                handle.rt.descriptor.useDynamicScaleExplicit != descriptor.useDynamicScaleExplicit ||
                handle.rt.descriptor.memoryless != descriptor.memoryless ||
                handle.rt.filterMode != descriptor.filterMode ||
                handle.rt.wrapMode != descriptor.wrapMode ||
                handle.rt.anisoLevel != descriptor.anisoLevel ||
                handle.rt.mipMapBias != descriptor.mipMapBias ||
                handle.name != descriptor.name;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static RTHandleAllocInfo CreateRTHandleAllocInfo(in RenderTextureDescriptor descriptor, FilterMode filterMode, TextureWrapMode wrapMode, int anisoLevel, float mipMapBias, string name)
        {
            var actualFormat = descriptor.graphicsFormat != GraphicsFormat.None ? descriptor.graphicsFormat : descriptor.depthStencilFormat;

            // NOTE: this calls default(RTHandleAllocInfo) not RTHandleAllocInfo(string = "")
            RTHandleAllocInfo allocInfo = new RTHandleAllocInfo();
            allocInfo.slices = descriptor.volumeDepth;
            allocInfo.format = actualFormat;
            allocInfo.filterMode = filterMode;
            allocInfo.wrapModeU = wrapMode;
            allocInfo.wrapModeV = wrapMode;
            allocInfo.wrapModeW = wrapMode;
            allocInfo.dimension = descriptor.dimension;
            allocInfo.enableRandomWrite = descriptor.enableRandomWrite;
            allocInfo.enableShadingRate = descriptor.enableShadingRate;
            allocInfo.useMipMap = descriptor.useMipMap;
            allocInfo.autoGenerateMips = descriptor.autoGenerateMips;
            allocInfo.anisoLevel = anisoLevel;
            allocInfo.mipMapBias = mipMapBias;
            allocInfo.isShadowMap = descriptor.shadowSamplingMode != ShadowSamplingMode.None;
            allocInfo.msaaSamples = (MSAASamples)descriptor.msaaSamples;
            allocInfo.bindTextureMS = descriptor.bindMS;
            allocInfo.useDynamicScale = descriptor.useDynamicScale;
            allocInfo.useDynamicScaleExplicit = descriptor.useDynamicScaleExplicit;
            allocInfo.memoryless = descriptor.memoryless;
            allocInfo.vrUsage = descriptor.vrUsage;
            allocInfo.enableShadingRate = descriptor.enableShadingRate;
            allocInfo.name = name;

            return allocInfo;
        }
    }
}