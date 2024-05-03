using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public static class LiteRPRenderGraphUtils 
    {
        public static bool IsSupportsNativeRenderPassRenderGraphCompiler()
        {
            return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12
                   && SystemInfo.graphicsDeviceType !=
                   GraphicsDeviceType.OpenGLES3 // GLES doesn't support backbuffer MSAA resolve with the NRP API
                   && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore
                   && SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation5; // UUM-56295

        }
        
        // 创建RenderGraph管理的渲染纹理
        internal static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear,
            FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            TextureDesc rgDesc = new TextureDesc(desc.width, desc.height);
            rgDesc.dimension = desc.dimension;
            rgDesc.clearBuffer = clear;
            rgDesc.bindTextureMS = desc.bindMS;
            rgDesc.colorFormat = desc.graphicsFormat;
            rgDesc.depthBufferBits = (DepthBits)desc.depthBufferBits;
            rgDesc.slices = desc.volumeDepth;
            rgDesc.msaaSamples = (MSAASamples)desc.msaaSamples;
            rgDesc.name = name;
            rgDesc.enableRandomWrite = desc.enableRandomWrite;
            rgDesc.filterMode = filterMode;
            rgDesc.wrapMode = wrapMode;
            rgDesc.isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None && desc.depthStencilFormat != GraphicsFormat.None;
            rgDesc.vrUsage = desc.vrUsage;
            // TODO RENDERGRAPH: depthStencilFormat handling?

            return renderGraph.CreateTexture(rgDesc);
        }
        // 创建RenderGraph管理的渲染纹理
        internal static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear, Color color,
            FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            TextureDesc rgDesc = new TextureDesc(desc.width, desc.height);
            rgDesc.dimension = desc.dimension;
            rgDesc.clearBuffer = clear;
            rgDesc.clearColor = color;
            rgDesc.bindTextureMS = desc.bindMS;
            rgDesc.colorFormat = desc.graphicsFormat;
            rgDesc.depthBufferBits = (DepthBits)desc.depthBufferBits;
            rgDesc.slices = desc.volumeDepth;
            rgDesc.msaaSamples = (MSAASamples)desc.msaaSamples;
            rgDesc.name = name;
            rgDesc.enableRandomWrite = desc.enableRandomWrite;
            rgDesc.filterMode = filterMode;
            rgDesc.wrapMode = wrapMode;

            return renderGraph.CreateTexture(rgDesc);
        }
    }
}