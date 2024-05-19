using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.AdditionalData
{
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
        
        /// <summary>
        /// Light soft shadow filtering quality.
        /// </summary>
        [Tooltip("Controls the filtering quality of soft shadows. Higher quality has lower performance.")]
        public SoftShadowQuality softShadowQuality
        {
            get => m_SoftShadowQuality;
            set => m_SoftShadowQuality = value;
        }
        [SerializeField] private SoftShadowQuality m_SoftShadowQuality = SoftShadowQuality.UsePipelineSettings;

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
        }
    }
}