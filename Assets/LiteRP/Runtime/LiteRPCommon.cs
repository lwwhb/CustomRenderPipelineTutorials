using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace LiteRP
{
    public enum DepthFormat
    {
        /// <summary>
        /// Default format for Android and Switch platforms is <see cref="GraphicsFormat.D24_UNorm_S8_UInt"/> and <see cref="GraphicsFormat.D32_SFloat_S8_UInt"/> for other platforms
        /// </summary>
        Default,

        /// <summary>
        /// Format containing 16 unsigned normalized bits in depth component. Corresponds to <see cref="GraphicsFormat.D16_UNorm"/>.
        /// </summary>
        Depth_16 = GraphicsFormat.D16_UNorm,

        /// <summary>
        /// Format containing 24 unsigned normalized bits in depth component. Corresponds to <see cref="GraphicsFormat.D24_UNorm"/>.
        /// </summary>
        Depth_24 = GraphicsFormat.D24_UNorm,

        /// <summary>
        /// Format containing 32 signed float bits in depth component. Corresponds to <see cref="GraphicsFormat.D32_SFloat"/>.
        /// </summary>
        Depth_32 = GraphicsFormat.D32_SFloat,

        /// <summary>
        /// Format containing 16 unsigned normalized bits in depth component and 8 unsigned integer bits in stencil. Corresponds to <see cref="GraphicsFormat.D16_UNorm_S8_UInt"/>.
        /// </summary>
        Depth_16_Stencil_8 = GraphicsFormat.D16_UNorm_S8_UInt,

        /// <summary>
        /// Format containing 24 unsigned normalized bits in depth component and 8 unsigned integer bits in stencil. Corresponds to <see cref="GraphicsFormat.D24_UNorm_S8_UInt"/>.
        /// </summary>
        Depth_24_Stencil_8 = GraphicsFormat.D24_UNorm_S8_UInt,

        /// <summary>
        /// Format containing 32 signed float bits in depth component and 8 unsigned integer bits in stencil. Corresponds to <see cref="GraphicsFormat.D32_SFloat_S8_UInt"/>.
        /// </summary>
        Depth_32_Stencil_8 = GraphicsFormat.D32_SFloat_S8_UInt,
    }
    /// <summary>
    /// Options for setting MSAA Quality.
    /// This defines how many samples URP computes per pixel for evaluating the effect.
    /// </summary>
    public enum MsaaQuality
    {
        /// <summary>
        /// Disables MSAA.
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// Use this for 2 samples per pixel.
        /// </summary>
        _2x = 2,

        /// <summary>
        /// Use this for 4 samples per pixel.
        /// </summary>
        _4x = 4,

        /// <summary>
        /// Use this for 8 samples per pixel.
        /// </summary>
        _8x = 8
    }
    
    public enum ShadowQuality
    {
        /// <summary>
        /// Disables the shadows.
        /// </summary>
        Disabled,
        /// <summary>
        /// Shadows have hard edges.
        /// </summary>
        HardShadows,
        /// <summary>
        /// Filtering is applied when sampling shadows. Shadows have smooth edges.
        /// </summary>
        SoftShadows,
    }

    /// <summary>
    /// Softness quality of soft shadows. Higher means better quality, but lower performance.
    /// </summary>
    public enum SoftShadowQuality
    {
        /// <summary>
        /// Use this to choose the setting set on the pipeline asset.
        /// </summary>
        [InspectorName("Use settings from Render Pipeline Asset")]
        UsePipelineSettings,

        /// <summary>
        /// Low quality soft shadows. Recommended for mobile. 4 PCF sample filtering.
        /// </summary>
        Low,
        /// <summary>
        /// Medium quality soft shadows. The default. 5x5 tent filtering.
        /// </summary>
        Medium,
        /// <summary>
        /// High quality soft shadows. Low performance due to high sample count. 7x7 tent filtering.
        /// </summary>
        High,
    }

    /// <summary>
    /// This controls the size of the shadow map texture.
    /// </summary>
    public enum ShadowResolution
    {
        /// <summary>
        /// Use this for 256x256 shadow resolution.
        /// </summary>
        _256 = 256,

        /// <summary>
        /// Use this for 512x512 shadow resolution.
        /// </summary>
        _512 = 512,

        /// <summary>
        /// Use this for 1024x1024 shadow resolution.
        /// </summary>
        _1024 = 1024,

        /// <summary>
        /// Use this for 2048x2048 shadow resolution.
        /// </summary>
        _2048 = 2048,

        /// <summary>
        /// Use this for 4096x4096 shadow resolution.
        /// </summary>
        _4096 = 4096,

        /// <summary>
        /// Use this for 8192x8192 shadow resolution.
        /// </summary>
        _8192 = 8192,
    }
    
    /// <summary>
    /// Defines the update frequency for the Volume Framework.
    /// </summary>
    public enum VolumeFrameworkUpdateMode
    {
        /// <summary>
        /// Use this to have the Volume Framework update every frame.
        /// </summary>
        [InspectorName("Every Frame")]
        EveryFrame = 0,

        /// <summary>
        /// Use this to disable Volume Framework updates or to update it manually via scripting.
        /// </summary>
        [InspectorName("Via Scripting")]
        ViaScripting = 1,

        /// <summary>
        /// Use this to choose the setting set on the pipeline asset.
        /// </summary>
        [InspectorName("Use Pipeline Settings")]
        UsePipelineSettings = 2,
    }
    
    internal enum DefaultMaterialType
    {
        Default,
        Particle
    }
}