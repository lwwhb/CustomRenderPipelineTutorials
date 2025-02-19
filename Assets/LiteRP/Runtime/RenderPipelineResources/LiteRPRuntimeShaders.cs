using System;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace LiteRP
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [CategoryInfo(Name = "R: Runtime Shaders", Order = 1000), HideInInspector]
    public class LiteRPRuntimeShaders : IRenderPipelineResources
    {
        public int version => 0;
        
        [SerializeField]
        [ResourcePath("LiteRP/Shaders/Hidden/CopyDepth.shader")]
        private Shader m_CopyDepthPS;

        /// <summary>
        /// Shader used to copy the depth buffer.
        /// </summary>
        public Shader copyDepthPS
        {
            get => m_CopyDepthPS;
            set => this.SetValueAndNotify(ref m_CopyDepthPS, value, nameof(m_CopyDepthPS));
        }
        
        [SerializeField]
        [ResourcePath("LiteRP/Shaders/Hidden/CoreBlit.shader")]
        internal Shader m_CoreBlitPS;

        /// <summary>
        /// Default blit shader used for blit operation.
        /// </summary>
        public Shader coreBlitPS
        {
            get => m_CoreBlitPS;
            set => this.SetValueAndNotify(ref m_CoreBlitPS, value, nameof(m_CoreBlitPS));
        }
        
        [SerializeField]
        [ResourcePath("LiteRP/Shaders/Hidden/BlitHDROverlay.shader")]
        internal Shader m_BlitHDROverlay;
        
        /// <summary>
        /// Blit shader used for HDR Overlay.
        /// </summary>
        public Shader blitHDROverlay
        {
            get => m_BlitHDROverlay;
            set => this.SetValueAndNotify(ref m_BlitHDROverlay, value, nameof(m_BlitHDROverlay));
        }
        
        [SerializeField]
        [ResourcePath("LiteRP/Shaders/Hidden/Sampling.shader")]
        private Shader m_SamplingPS;

        /// <summary>
        /// Shader used when sampling is required.
        /// </summary>
        public Shader samplingPS
        {
            get => m_SamplingPS;
            set => this.SetValueAndNotify(ref m_SamplingPS, value, nameof(m_SamplingPS));
        }
    }
}
