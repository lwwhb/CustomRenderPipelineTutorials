using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace LiteRP.Editor
{
    internal class LitShaderGUI : LiteRPShaderGUI
    {
        static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitShaderHelper.WorkflowMode));
        // The workflow mode Props.
        protected MaterialProperty m_WorkflowModeProperty{ get; set; }
        
        // Surface Input Props
        protected MaterialProperty m_MetallicProperty { get; set; }
        protected MaterialProperty m_SpecularProperty { get; set; }
        protected MaterialProperty m_MetallicGlossMapProperty { get; set; }
        protected MaterialProperty m_SpecularGlossMapProperty { get; set; }
        protected MaterialProperty m_SmoothnessProperty { get; set; }
        protected MaterialProperty m_SmoothnessMapChannelProperty { get; set; }
        protected MaterialProperty m_BumpMapProperty { get; set; }
        protected MaterialProperty m_BumpScaleProperty { get; set; }
        protected MaterialProperty m_ParallaxMapProperty { get; set; }
        protected MaterialProperty m_ParallaxScaleProperty { get; set; }
        protected MaterialProperty m_OcclusionStrengthProperty { get; set; }
        protected MaterialProperty m_OcclusionMapProperty { get; set; }


        public MaterialProperty m_ClearCoatProperty { get; set; }
        public MaterialProperty m_ClearCoatMapProperty { get; set; }
        public MaterialProperty m_ClearCoatMaskProperty { get; set; }
        public MaterialProperty m_ClearCoatSmoothnessProperty { get; set; }

        // Advanced Props
        protected MaterialProperty m_HighlightsProperty { get; set; }
        protected MaterialProperty m_ReflectionsProperty { get; set; }
        protected MaterialProperty m_OptimizedBRDFProperty { get; set; }
        
            
        public override void ValidateMaterial(Material material)
        {
            LiteRPShaderHelper.SetMaterialKeywords(material, LitShaderHelper.SetMaterialKeywords);
        }
        
        protected override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            
            // Surface Option Props
            m_WorkflowModeProperty = FindProperty(LiteRPShaderProperty.WorkflowMode, properties, false);
            
            // Surface Input Props
            m_MetallicProperty = FindProperty(LiteRPShaderProperty.Metallic, properties);
            m_SpecularProperty = FindProperty(LiteRPShaderProperty.SpecColor, properties,false);
            m_MetallicGlossMapProperty = FindProperty(LiteRPShaderProperty.MetallicGlossMap, properties);
            m_SpecularGlossMapProperty = FindProperty(LiteRPShaderProperty.SpecGlossMap, properties, false);
            m_SmoothnessProperty = FindProperty(LiteRPShaderProperty.Smoothness, properties, false);
            m_SmoothnessMapChannelProperty = FindProperty(LiteRPShaderProperty.SmoothnessTextureChannel, properties, false);
            m_BumpMapProperty = FindProperty(LiteRPShaderProperty.NormalMap, properties, false);
            m_BumpScaleProperty = FindProperty(LiteRPShaderProperty.NormalScale, properties, false);
            m_ParallaxMapProperty = FindProperty(LiteRPShaderProperty.ParallaxMap, properties, false);
            m_ParallaxScaleProperty = FindProperty(LiteRPShaderProperty.Parallax, properties, false);
            m_OcclusionStrengthProperty = FindProperty(LiteRPShaderProperty.OcclusionStrength, properties, false);
            m_OcclusionMapProperty = FindProperty(LiteRPShaderProperty.OcclusionMap, properties, false);
            
            m_ClearCoatProperty = FindProperty(LiteRPShaderProperty.ClearCoat, properties, false);
            m_ClearCoatMapProperty = FindProperty(LiteRPShaderProperty.ClearCoatMap, properties, false);
            m_ClearCoatMaskProperty = FindProperty(LiteRPShaderProperty.ClearCoatMask, properties, false);
            m_ClearCoatSmoothnessProperty = FindProperty(LiteRPShaderProperty.ClearCoatSmoothness, properties, false);
            
            // Advanced Props
            m_HighlightsProperty = FindProperty(LiteRPShaderProperty.SpecularHighlights, properties, false);
            m_ReflectionsProperty = FindProperty(LiteRPShaderProperty.EnvironmentReflections, properties, false);
            m_OptimizedBRDFProperty = FindProperty(LiteRPShaderProperty.OptimizedBRDF, properties, false);
        }
        
        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                LiteRPShaderHelper.SetupMaterialBlendMode(material);
                return;
            }
            
            SurfaceType surfaceType = (SurfaceType)material.GetFloat(LiteRPShaderProperty.SurfaceType);
            BlendMode blendMode = (BlendMode)material.GetFloat(LiteRPShaderProperty.BlendMode);
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat(LiteRPShaderProperty.AlphaClip, 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat(LiteRPShaderProperty.BlendMode, (float)blendMode);
            material.SetFloat(LiteRPShaderProperty.SurfaceType, (float)surfaceType);
            
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword(ShaderKeywordStrings.SurfaceTypeTransparent);
            }
            else
            {
                material.EnableKeyword(ShaderKeywordStrings.SurfaceTypeTransparent);
            }
            
            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat(LiteRPShaderProperty.WorkflowMode, (float)LitShaderHelper.WorkflowMode.Specular);
                Texture texture = material.GetTexture(LiteRPShaderProperty.SpecGlossMap);
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat(LiteRPShaderProperty.WorkflowMode, (float)LitShaderHelper.WorkflowMode.Metallic);
                Texture texture = material.GetTexture(LiteRPShaderProperty.MetallicGlossMap);
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (m_WorkflowModeProperty != null)
                m_MaterialEditor.PopupShaderProperty(m_WorkflowModeProperty, LitShaderGUIHelper.Styles.workflowModeText,workflowModeNames);

            base.DrawSurfaceOptions(material);
        }
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitShaderGUIHelper.DrawMetallicSpecularProperties(m_MaterialEditor, material, m_WorkflowModeProperty,
                m_MetallicProperty, m_MetallicGlossMapProperty, 
                m_SpecularProperty, m_SpecularGlossMapProperty, 
                m_SmoothnessProperty, m_SmoothnessMapChannelProperty);
            LiteRPShaderGUIHelper.DrawNormalProperties(m_MaterialEditor, m_BumpMapProperty, m_BumpScaleProperty);

            if (m_ParallaxMapProperty != null && m_ParallaxScaleProperty != null)
                LitShaderGUIHelper.DrawHeightProperties(m_MaterialEditor, m_ParallaxMapProperty, m_ParallaxScaleProperty);

            if (m_OcclusionMapProperty != null && m_OcclusionStrengthProperty != null)
                LitShaderGUIHelper.DrawOcclusionProperties(m_MaterialEditor, m_OcclusionMapProperty, m_OcclusionStrengthProperty);
            if (m_EmissionMapProperty != null && m_EmissionColorProperty != null)
                LiteRPShaderGUIHelper.DrawEmissionProperties(m_MaterialEditor, m_EmissionMapProperty, m_EmissionColorProperty, true);
            LiteRPShaderGUIHelper.DrawTileOffset(m_MaterialEditor, m_BaseMapProperty);
            if(m_ClearCoatProperty != null && m_ClearCoatMapProperty != null && m_ClearCoatMaskProperty != null && m_ClearCoatSmoothnessProperty != null)
                LitShaderGUIHelper.DrawClearCoatProperties(m_MaterialEditor, m_ClearCoatProperty, m_ClearCoatMapProperty, m_ClearCoatMaskProperty, m_ClearCoatSmoothnessProperty);
        }
        
        public override void DrawAdvancedOptions(Material material)
        {
            if (m_ReflectionsProperty != null && m_HighlightsProperty != null)
            {
                m_MaterialEditor.ShaderProperty(m_HighlightsProperty, LitShaderGUIHelper.Styles.highlightsText);
                m_MaterialEditor.ShaderProperty(m_ReflectionsProperty, LitShaderGUIHelper.Styles.reflectionsText);
                m_MaterialEditor.ShaderProperty(m_OptimizedBRDFProperty, LitShaderGUIHelper.Styles.optimizedBRDFText);
            }

            base.DrawAdvancedOptions(material);
        }
    }
}
