using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.AdditionalData
{
    public enum LightLayerEnum
    {
        /// <summary>The light will no affect any object.</summary>
        Nothing = 0,   // Custom name for "Nothing" option
        /// <summary>Light Layer 0.</summary>
        LightLayerDefault = 1 << 0,
        /// <summary>Light Layer 1.</summary>
        LightLayer1 = 1 << 1,
        /// <summary>Light Layer 2.</summary>
        LightLayer2 = 1 << 2,
        /// <summary>Light Layer 3.</summary>
        LightLayer3 = 1 << 3,
        /// <summary>Light Layer 4.</summary>
        LightLayer4 = 1 << 4,
        /// <summary>Light Layer 5.</summary>
        LightLayer5 = 1 << 5,
        /// <summary>Light Layer 6.</summary>
        LightLayer6 = 1 << 6,
        /// <summary>Light Layer 7.</summary>
        LightLayer7 = 1 << 7,
        /// <summary>Everything.</summary>
        Everything = 0xFF, // Custom name for "Everything" option
    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class AdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
    {
        [Tooltip("Controls if light Shadow Bias parameters use pipeline settings.")]
        [SerializeField] bool m_UsePipelineSettings = true;
        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }
        public static readonly int AdditionalLightsShadowResolutionTierCustom = -1;
        public static readonly int AdditionalLightsShadowResolutionTierLow = 0;
        public static readonly int AdditionalLightsShadowResolutionTierMedium = 1;
        public static readonly int AdditionalLightsShadowResolutionTierHigh = 2;
        public static readonly int AdditionalLightsShadowDefaultResolutionTier = AdditionalLightsShadowResolutionTierHigh;

        /// <summary>
        /// The default custom shadow resolution for additional lights.
        /// </summary>
        public static readonly int AdditionalLightsShadowDefaultCustomResolution = 128;
        
        
        [NonSerialized] private Light m_Light;

        /// <summary>
        /// Returns the cached light component associated with the game object that owns this light data.
        /// </summary>
#if UNITY_EDITOR
        internal new Light light
#else
        internal Light light
#endif
        {
            get
            {
                if (!m_Light)
                    TryGetComponent(out m_Light);
                return m_Light;
            }
        }
        
        public static readonly int AdditionalLightsShadowMinimumResolution = 128;

        [Tooltip("Controls if light shadow resolution uses pipeline settings.")]
        [SerializeField] int m_AdditionalLightsShadowResolutionTier = AdditionalLightsShadowDefaultResolutionTier;
        public int additionalLightsShadowResolutionTier
        {
            get { return m_AdditionalLightsShadowResolutionTier; }
        }
        
        [SerializeField] uint m_RenderingLayers = 1;

        /// <summary>
        /// Specifies which rendering layers this light will affect.
        /// </summary>
        public uint renderingLayers
        {
            get
            {
                return m_RenderingLayers;
            }
            set
            {
                if (m_RenderingLayers != value)
                {
                    m_RenderingLayers = value;
                    SyncLightAndShadowLayers();
                }
            }
        }
        
        [SerializeField] bool m_CustomShadowLayers = false;

        /// <summary>
        /// Indicates whether shadows need custom layers.
        /// If not, then it uses the same settings as lightLayerMask.
        /// </summary>
        public bool customShadowLayers
        {
            get
            {
                return m_CustomShadowLayers;
            }
            set
            {
                if (m_CustomShadowLayers != value)
                {
                    m_CustomShadowLayers = value;
                    SyncLightAndShadowLayers();
                }
            }
        }
        
        [SerializeField] LightLayerEnum m_ShadowLayerMask = LightLayerEnum.LightLayerDefault;
        
        
        [SerializeField] uint m_ShadowRenderingLayers = 1;
        /// <summary>
        /// Specifies which rendering layers this light shadows will affect.
        /// </summary>
        public uint shadowRenderingLayers
        {
            get
            {
                return m_ShadowRenderingLayers;
            }
            set
            {
                if (value != m_ShadowRenderingLayers)
                {
                    m_ShadowRenderingLayers = value;
                    SyncLightAndShadowLayers();
                }
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
        }
        
        private void SyncLightAndShadowLayers()
        {
            if (light)
                light.renderingLayerMask = m_CustomShadowLayers ? (int)m_ShadowRenderingLayers : (int)m_RenderingLayers;
        }
    }
}