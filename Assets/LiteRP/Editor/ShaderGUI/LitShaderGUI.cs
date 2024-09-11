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
        protected MaterialProperty m_BumpMapPropProperty { get; set; }
        protected MaterialProperty m_BumpScalePropProperty { get; set; }
        protected MaterialProperty m_ParallaxMapPropProperty { get; set; }
        protected MaterialProperty m_ParallaxScalePropProperty { get; set; }
        protected MaterialProperty m_OcclusionStrengthProperty { get; set; }
        protected MaterialProperty m_OcclusionMapProperty { get; set; }
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
            m_WorkflowModeProperty = FindProperty(LiteRPShaderProperty.SpecularWorkflowMode, properties, false);
            
            // Surface Input Props
            m_MetallicProperty = FindProperty(LiteRPShaderProperty.Metallic, properties);
            m_SpecularProperty = FindProperty(LiteRPShaderProperty.SpecColor, properties,false);
            m_MetallicGlossMapProperty = FindProperty(LiteRPShaderProperty.MetallicGlossMap, properties);
            m_SpecularGlossMapProperty = FindProperty(LiteRPShaderProperty.SpecGlossMap, properties, false);
            m_SmoothnessProperty = FindProperty(LiteRPShaderProperty.Smoothness, properties, false);
            m_SmoothnessMapChannelProperty = FindProperty(LiteRPShaderProperty.SmoothnessTextureChannel, properties, false);
            m_BumpMapPropProperty = FindProperty(LiteRPShaderProperty.NormalMap, properties, false);
            m_BumpScalePropProperty = FindProperty(LiteRPShaderProperty.NormalScale, properties, false);
            m_ParallaxMapPropProperty = FindProperty(LiteRPShaderProperty.ParallaxMap, properties, false);
            m_ParallaxScalePropProperty = FindProperty(LiteRPShaderProperty.Parallax, properties, false);
            m_OcclusionStrengthProperty = FindProperty(LiteRPShaderProperty.OcclusionStrength, properties, false);
            m_OcclusionMapProperty = FindProperty(LiteRPShaderProperty.OcclusionMap, properties, false);
            
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
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);
            material.SetFloat("_Surface", (float)surfaceType);
            
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
                material.SetFloat("_WorkflowMode", (float)LitShaderHelper.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitShaderHelper.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
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
            LiteRPShaderGUIHelper.DrawNormalProperties(m_MaterialEditor, m_BumpMapPropProperty, m_BumpScalePropProperty);

            if (m_ParallaxMapPropProperty != null && m_ParallaxScalePropProperty != null)
                LitShaderGUIHelper.DrawHeightProperties(m_MaterialEditor, m_ParallaxMapPropProperty, m_ParallaxScalePropProperty);

            if (m_OcclusionMapProperty != null && m_OcclusionStrengthProperty != null)
                LitShaderGUIHelper.DrawOcclusionProperties(m_MaterialEditor, m_OcclusionMapProperty, m_OcclusionStrengthProperty);
            
            LiteRPShaderGUIHelper.DrawEmissionProperties(m_MaterialEditor, m_EmissionMapProperty, m_EmissionColorProperty, true);
            LiteRPShaderGUIHelper.DrawTileOffset(m_MaterialEditor, m_BaseMapProperty);
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
