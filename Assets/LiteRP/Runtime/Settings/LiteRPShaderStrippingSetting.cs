using System;
using LiteRP;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Settings
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    public class LiteRPShaderStrippingSetting : IRenderPipelineGraphicsSettings
    {
        #region Version
        internal enum Version : int
        {
            Initial = 0,
        }
        [SerializeField][HideInInspector]
        private Version m_Version;
        public int version => (int)m_Version;
        #endregion
        
        #region SerializeFields
        [SerializeField]
        [Tooltip("Controls whether to strip variants if the feature is disabled.")]
        bool m_StripUnusedVariants = true;
        #endregion
        
        #region Data Accessors
        public bool stripUnusedVariants
        {
            get => m_StripUnusedVariants;
            set => this.SetValueAndNotify(ref m_StripUnusedVariants, value);
        }
        #endregion
    }
}