
using System;
using LiteRP.RenderPipelineResources;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;
#endif
namespace LiteRP
{
    [CreateAssetMenu(menuName = "Lite Render Pipeline/Lite Render Pipeline Asset")]
#if UNITY_EDITOR
    [ShaderKeywordFilter.ApplyRulesIfTagsEqual("RenderPipeline", "LiteRenderPipeline")]
#endif
    public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipeline>
    {
        //lwwhb 一定要复写
        public override string renderPipelineShaderTag => LiteRenderPipeline.k_ShaderTagName;
        
        // RenderPipelineSettings
        #region RenderPipelineSettings
        [SerializeField] bool m_UseSRPBatcher = true;
        public bool useSRPBatcher
        {
            get => m_UseSRPBatcher;
            set => m_UseSRPBatcher = value;
        }
       
        [SerializeField] GPUResidentDrawerMode m_GPUResidentDrawerMode = GPUResidentDrawerMode.Disabled;
        public GPUResidentDrawerMode gpuResidentDrawerMode
        {
            get => m_GPUResidentDrawerMode;
            set
            {
                if (value == m_GPUResidentDrawerMode)
                    return;

                m_GPUResidentDrawerMode = value;
                OnValidate();
            }
        }
        
        [SerializeField] float m_SmallMeshScreenPercentage = 0.0f;
        public float smallMeshScreenPercentage
        {
            get => m_SmallMeshScreenPercentage;
            set
            {
                if (Math.Abs(value - m_SmallMeshScreenPercentage) < float.Epsilon)
                    return;

                m_SmallMeshScreenPercentage = Mathf.Clamp(value, 0.0f, 20.0f);
                OnValidate();
            }
        }

        [SerializeField] bool m_GPUResidentDrawerEnableOcclusionCullingInCameras;
        public bool gpuResidentDrawerEnableOcclusionCullingInCameras
        {
            get => m_GPUResidentDrawerEnableOcclusionCullingInCameras;
            set
            {
                if (value == m_GPUResidentDrawerEnableOcclusionCullingInCameras)
                    return;

                m_GPUResidentDrawerEnableOcclusionCullingInCameras = value;
                OnValidate();
            }
        }
        #endregion
        
        // QulitySettings
        #region QulitySettings
        [SerializeField] bool m_SupportsHDR = true;
        public bool supportsHDR
        {
            get => m_SupportsHDR;
            set => m_SupportsHDR = value;
        }
        
        [SerializeField] MsaaQuality m_MSAA = MsaaQuality.Disabled;
        public int msaaSampleCount
        {
            get => (int)m_MSAA;
            set => m_MSAA = (MsaaQuality)value;
        }
        
        [SerializeField] int m_AntiAliasing = 1;
        public int antiAliasing
        {
            get => m_AntiAliasing;
            set => m_AntiAliasing = value;
        }
        
        [SerializeField] bool m_ConservativeEnclosingSphere = true;
        public bool conservativeEnclosingSphere
        {
            get => m_ConservativeEnclosingSphere;
            set => m_ConservativeEnclosingSphere = value;
        }
        [SerializeField] int m_NumIterationsEnclosingSphere = 64;
        public int numIterationsEnclosingSphere
        {
            get => m_NumIterationsEnclosingSphere;
            set => m_NumIterationsEnclosingSphere = value;
        }
        #endregion
        
        // LightSettings
        #region LightSettings
        internal const int k_MaxPerObjectLights = 8;
        [SerializeField] int m_AdditionalLightsPerObjectLimit = 4;
        public int maxAdditionalLightsCount
        {
            get => m_AdditionalLightsPerObjectLimit;
            set => m_AdditionalLightsPerObjectLimit = Math.Max(0, System.Math.Min(value, k_MaxPerObjectLights));
        }
        #endregion
        
        // ShadowSettings
        #region ShadowSettings
        [SerializeField] bool m_MainLightShadowEnabled = true;
        public bool mainLightShadowEnabled
        {
            get => m_MainLightShadowEnabled;
            internal set
            {
                m_MainLightShadowEnabled = value;
#if UNITY_EDITOR
                m_AnyShadowsSupported = m_MainLightShadowEnabled;
#endif
            }
        }

        [SerializeField] ShadowResolution m_MainLightShadowmapResolution = ShadowResolution._2048;
        public int mainLightShadowmapResolution
        {
            get => (int)m_MainLightShadowmapResolution;
            set => m_MainLightShadowmapResolution = (ShadowResolution)value;
        }
        
        [SerializeField] float m_MainLightShadowDistance = 50;
        public float mainLightShadowDistance
        {
            get => m_MainLightShadowDistance;
            set => m_MainLightShadowDistance = Mathf.Max(0.0f, value);
        }
        
        internal const int k_ShadowCascadeMinCount = 1;
        internal const int k_ShadowCascadeMaxCount = 4;
        [SerializeField] int m_MainLightShadowCascadesCount = k_ShadowCascadeMaxCount;
        public int mainLightShadowCascadesCount
        {
            get => m_MainLightShadowCascadesCount;
            set
            {
                if (value < k_ShadowCascadeMinCount || value > k_ShadowCascadeMaxCount)
                {
                    throw new ArgumentException($"Value ({value}) needs to be between {k_ShadowCascadeMinCount} and {k_ShadowCascadeMaxCount}.");
                }
                m_MainLightShadowCascadesCount = value;
            }
        }
        
        [SerializeField] float m_MainLightCascade2Split = 0.25f;
        public float mainLightCascade2Split
        {
            get => m_MainLightCascade2Split;
            set => m_MainLightCascade2Split = value;
        }
        [SerializeField] Vector2 m_MainLightCascade3Split = new Vector2(0.1f, 0.3f);
        public Vector2 mainLightCascade3Split
        {
            get => m_MainLightCascade3Split;
            set => m_MainLightCascade3Split = value;
        }
        [SerializeField] Vector3 m_MainLightCascade4Split = new Vector3(0.067f, 0.2f, 0.467f);
        public Vector3 mainLightCascade4Split
        {
            get => m_MainLightCascade4Split;
            set => m_MainLightCascade4Split = value;
        }
        [SerializeField] float m_MainLightCascadeBorder = 0.2f;
        public float mainLightCascadeBorder
        {
            get => m_MainLightCascadeBorder;
            set => m_MainLightCascadeBorder = value;
        }
        internal const float k_MaxShadowBias = 10.0f;
        [SerializeField] float m_MainLightShadowDepthBias = 1.0f;
        public float mainLightShadowDepthBias
        {
            get => m_MainLightShadowDepthBias;
            set => m_MainLightShadowDepthBias = Mathf.Max(0.0f, Mathf.Min(value, k_MaxShadowBias));
        }
        [SerializeField] float m_MainLightShadowNormalBias = 1.0f;
        public float mainLightShadowNormalBias
        {
            get => m_MainLightShadowNormalBias;
            set => m_MainLightShadowNormalBias = Mathf.Max(0.0f, Mathf.Min(value, k_MaxShadowBias));
        }
#if UNITY_EDITOR // multi_compile_fragment _ _SHADOWS_SOFT
        [ShaderKeywordFilter.RemoveIf(false, keywordNames: ShaderKeywordStrings.SoftShadows)]
        [SerializeField] bool m_AnyShadowsSupported = true;
        // No option to force soft shadows -> we'll need to keep the off variant around
        [ShaderKeywordFilter.RemoveIf(false, keywordNames: ShaderKeywordStrings.SoftShadows)]
#endif
        [SerializeField] bool m_SoftShadowsSupported = false;
        public bool supportsSoftShadows
        {
            get => m_SoftShadowsSupported;
            internal set => m_SoftShadowsSupported = value;
        }
        [SerializeField] SoftShadowQuality m_SoftShadowQuality = SoftShadowQuality.Medium;
        public SoftShadowQuality softShadowQuality
        {
            get => m_SoftShadowQuality;
            set => m_SoftShadowQuality = value;
        }
        #endregion
        
        // OtherSettings
        #region OtherSettings
        /// <summary>
        /// Returns the selected update mode for volumes.
        /// </summary>
        [SerializeField] VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateMode = VolumeFrameworkUpdateMode.EveryFrame;
        public VolumeFrameworkUpdateMode volumeFrameworkUpdateMode => m_VolumeFrameworkUpdateMode;

        /// <summary>
        /// A volume profile that can be used to override global default volume profile values. This provides a way e.g.
        /// to have different volume default values per quality level without having to place global volumes in scenes.
        /// </summary>
        [SerializeField] VolumeProfile m_VolumeProfile;
        public VolumeProfile volumeProfile
        {
            get => m_VolumeProfile;
            set => m_VolumeProfile = value;
        }
        #endregion
        
        // Default Materials
        #region Materials
        Material GetMaterial(DefaultMaterialType materialType)
        {
#if UNITY_EDITOR
            if (GraphicsSettings.TryGetRenderPipelineSettings<LiteRPEditorMaterials>(out var defaultMaterials))
            {
                return materialType switch
                {
                    DefaultMaterialType.Default => defaultMaterials.defaultMaterial,
                    DefaultMaterialType.Particle => defaultMaterials.defaultParticleUnlitMaterial,
                    _ => null
                };
            }
            return null;
#else
            return null;
#endif
        }

        /// <summary>
        /// Returns the default Material.
        /// </summary>
        /// <returns>Returns the default Material.</returns>
        public override Material defaultMaterial => GetMaterial(DefaultMaterialType.Default);
        /// <summary>
        /// Returns the default particle Material.
        /// </summary>
        /// <returns>Returns the default particle Material.</returns>
        public override Material defaultParticleMaterial => GetMaterial(DefaultMaterialType.Particle);

        /// <summary>
        /// Returns the default line Material.
        /// </summary>
        /// <returns>Returns the default line Material.</returns>
        public override Material defaultLineMaterial => GetMaterial(DefaultMaterialType.Particle);
        #endregion
        
        
        // Default Shaders
        #region Shaders
#if UNITY_EDITOR
        private LiteRPEditorShaders defaultShaders =>
            GraphicsSettings.GetRenderPipelineSettings<LiteRPEditorShaders>();
#endif

        Shader m_DefaultShader;

        /// <summary>
        /// Returns the default shader for the specified renderer. When creating new objects in the editor, the materials of those objects will use the selected default shader.
        /// </summary>
        /// <returns>Returns the default shader for the specified renderer.</returns>
        public override Shader defaultShader
        {
            get
            {
#if UNITY_EDITOR
                // TODO: When importing project, AssetPreviewUpdater:CreatePreviewForAsset will be called multiple time
                // which in turns calls this property to get the default shader.
                // The property should never return null as, when null, it loads the data using AssetDatabase.LoadAssetAtPath.
                // However it seems there's an issue that LoadAssetAtPath will not load the asset in some cases. so adding the null check
                // here to fix template tests.
                if (m_DefaultShader == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(ShaderUtils.GetShaderGUID(ShaderPathID.Lit));
                    m_DefaultShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                }
#endif

                if (m_DefaultShader == null)
                    m_DefaultShader = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.Lit));

                return m_DefaultShader;
            }
        }
        #endregion
        
        /// <summary>
        /// Ensures Global Settings are ready and registered into GraphicsSettings
        /// </summary>
        protected override void EnsureGlobalSettings()
        {
            base.EnsureGlobalSettings();
#if UNITY_EDITOR
            LiteRPGlobalSettings.Ensure();
#endif
        }
        
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline(this);
        }
    }
}
