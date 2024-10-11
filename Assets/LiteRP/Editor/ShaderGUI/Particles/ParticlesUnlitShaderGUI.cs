
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace LiteRP.Editor
{
    internal class ParticlesUnlitShaderGUI : LiteRPShaderGUI
    {
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
        // Surface Option Props

        /// <summary>
        /// The MaterialProperty for color mode.
        /// </summary>
        protected MaterialProperty m_ColorModeProperty { get; set; }
        
        // Materials Props
        protected MaterialProperty m_BumpMapProperty { get; set; }
        protected MaterialProperty m_BumpScaleProperty { get; set; }
        
        // Advanced Props

        /// <summary>
        /// The MaterialProperty for flip-book blending.
        /// </summary>
        protected MaterialProperty m_FlipbookModeProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for soft particles enabled.
        /// </summary>
        protected MaterialProperty m_SoftParticlesEnabledProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for camera fading.
        /// </summary>
        protected MaterialProperty m_CameraFadingEnabledProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for distortion enabled.
        /// </summary>
        protected MaterialProperty m_DistortionEnabledProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for soft particles near fade distance.
        /// </summary>
        protected MaterialProperty m_SoftParticlesNearFadeDistanceProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for soft particles far fade distance.
        /// </summary>
        protected MaterialProperty m_SoftParticlesFarFadeDistanceProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for camera fading near distance.
        /// </summary>
        protected MaterialProperty m_CameraNearFadeDistanceProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for camera fading far distance.
        /// </summary>
        protected MaterialProperty m_CameraFarFadeDistanceProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for distortion blend.
        /// </summary>
        protected MaterialProperty m_DistortionBlendProperty { get; set; }

        /// <summary>
        /// The MaterialProperty for distortion strength.
        /// </summary>
        protected MaterialProperty m_DistortionStrengthProperty { get; set; }
        
        
        public override void ValidateMaterial(Material material)
        {
            LiteRPShaderHelper.SetMaterialKeywords(material, null, ParticlesShaderHelper.SetMaterialKeywords);
        }

        protected override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            m_ColorModeProperty = FindProperty(LiteRPShaderProperty.ColorMode, properties, false);
            m_BumpMapProperty = FindProperty(LiteRPShaderProperty.NormalMap, properties, false);
            m_BumpScaleProperty = FindProperty(LiteRPShaderProperty.NormalScale, properties, false);
            m_FlipbookModeProperty = FindProperty(LiteRPShaderProperty.FlipbookMode, properties, false);
            m_SoftParticlesEnabledProperty = FindProperty(LiteRPShaderProperty.SoftParticlesEnabled, properties, false);
            m_CameraFadingEnabledProperty = FindProperty(LiteRPShaderProperty.CameraFadingEnabled, properties, false);
            m_DistortionEnabledProperty = FindProperty(LiteRPShaderProperty.DistortionEnabled, properties, false);
            m_SoftParticlesNearFadeDistanceProperty = FindProperty(LiteRPShaderProperty.SoftParticlesNearFadeDistance, properties, false);
            m_SoftParticlesFarFadeDistanceProperty = FindProperty(LiteRPShaderProperty.SoftParticlesFarFadeDistance, properties, false);
            m_CameraNearFadeDistanceProperty = FindProperty(LiteRPShaderProperty.CameraNearFadeDistance, properties, false);
            m_CameraFarFadeDistanceProperty = FindProperty(LiteRPShaderProperty.CameraFarFadeDistance, properties, false);
            m_DistortionBlendProperty = FindProperty(LiteRPShaderProperty.DistortionBlend, properties, false);
            m_DistortionStrengthProperty = FindProperty(LiteRPShaderProperty.DistortionStrength, properties, false);
        }
        
        public override void DrawSurfaceOptions(Material material)
        {
            if(m_SurfaceTypeProperty != null)
                m_MaterialEditor.PopupShaderProperty(m_SurfaceTypeProperty, LiteRPShaderGUIHelper.Styles.surfaceType, LiteRPShaderGUIHelper.Styles.surfaceTypeNames);
            if ((m_SurfaceTypeProperty != null) && ((SurfaceType)m_SurfaceTypeProperty.floatValue == SurfaceType.Transparent))
            {
                m_MaterialEditor.PopupShaderProperty(m_BlendModeProperty, LiteRPShaderGUIHelper.Styles.blendingMode, LiteRPShaderGUIHelper.Styles.blendModeNames);
                if (material.HasProperty(LiteRPShaderProperty.BlendModePreserveSpecular))
                {
                    BlendMode blendMode = (BlendMode)material.GetFloat(LiteRPShaderProperty.BlendMode);
                    var isDisabled = blendMode == BlendMode.Multiply || blendMode == BlendMode.Premultiply;
                    if (!isDisabled)
                        LiteRPShaderGUIHelper.DrawFloatToggleProperty(LiteRPShaderGUIHelper.Styles.preserveSpecularText, m_PreserveSpecProperty, 1, isDisabled);
                }
            }
            if(m_CullingProperty != null)
                m_MaterialEditor.PopupShaderProperty(m_CullingProperty, LiteRPShaderGUIHelper.Styles.cullingText, LiteRPShaderGUIHelper.Styles.renderFaceNames);
            if(m_ZWriteProperty != null)
                m_MaterialEditor.PopupShaderProperty(m_ZWriteProperty, LiteRPShaderGUIHelper.Styles.zwriteText, LiteRPShaderGUIHelper.Styles.zwriteNames);

            if (m_ZTestProperty != null)
                m_MaterialEditor.IntPopupShaderProperty(m_ZTestProperty, LiteRPShaderGUIHelper.Styles.ztestText.text, LiteRPShaderGUIHelper.Styles.ztestNames, LiteRPShaderGUIHelper.Styles.ztestValues);

            LiteRPShaderGUIHelper.DrawFloatToggleProperty(LiteRPShaderGUIHelper.Styles.alphaClipText, m_AlphaClipProperty);

            if ((m_AlphaClipProperty != null) && (m_AlphaCutoffProperty != null) && (m_AlphaClipProperty.floatValue == 1))
                m_MaterialEditor.ShaderProperty(m_AlphaCutoffProperty, LiteRPShaderGUIHelper.Styles.alphaClipThresholdText, 1);
            
            if (m_ColorModeProperty != null)
                m_MaterialEditor.PopupShaderProperty(m_ColorModeProperty, ParticlesShaderGUIHelper.Styles.colorMode, Enum.GetNames(typeof(ParticlesShaderHelper.ColorMode)));

            LiteRPShaderGUIHelper.DrawFloatToggleProperty(LiteRPShaderGUIHelper.Styles.castShadowText, m_CastShadowsProperty);
            LiteRPShaderGUIHelper.DrawFloatToggleProperty(LiteRPShaderGUIHelper.Styles.receiveShadowText, m_ReceiveShadowsProperty);
        }
        
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            //绘制BumpMap
            LiteRPShaderGUIHelper.DrawNormalProperties(m_MaterialEditor, m_BumpMapProperty, m_BumpScaleProperty);
            // 绘制EmissionMap
            LiteRPShaderGUIHelper.DrawEmissionProperties(m_MaterialEditor, m_EmissionMapProperty, m_EmissionColorProperty, true);
            LiteRPShaderGUIHelper.DrawTileOffset(m_MaterialEditor, m_BaseMapProperty);
        }
        public override void DrawAdvancedOptions(Material material)
        {
            if(m_FlipbookModeProperty != null)
                m_MaterialEditor.ShaderProperty(m_FlipbookModeProperty, ParticlesShaderGUIHelper.Styles.flipbookMode);
            ParticlesShaderGUIHelper.FadingOptions(material, m_MaterialEditor, 
                m_SoftParticlesEnabledProperty, m_SoftParticlesNearFadeDistanceProperty, m_SoftParticlesFarFadeDistanceProperty,
                m_CameraFadingEnabledProperty, m_CameraNearFadeDistanceProperty, m_CameraFarFadeDistanceProperty,
                m_DistortionEnabledProperty, m_DistortionStrengthProperty, m_DistortionBlendProperty);
            ParticlesShaderGUIHelper.DoVertexStreamsArea(material, m_RenderersUsingThisMaterial);

            bool autoQueueControl = LiteRPShaderHelper.GetAutomaticQueueControlSetting(material);
            if (autoQueueControl)
            {
                if (m_QueueOffsetProperty != null)
                    m_MaterialEditor.IntSliderShaderProperty(m_QueueOffsetProperty, -m_QueueOffsetRange, m_QueueOffsetRange, LiteRPShaderGUIHelper.Styles.queueSlider);
            }
        }

        protected override void InitializeShaderGUI(Material material, MaterialEditor materialEditorIn)
        {
            CacheRenderersUsingThisMaterial(material);
            base.InitializeShaderGUI(material, materialEditorIn);
        }

        private void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsByType<ParticleSystemRenderer>(FindObjectsSortMode.InstanceID);
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }
    }
}
