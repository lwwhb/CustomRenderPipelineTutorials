using UnityEngine;

namespace LiteRP
{
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
}