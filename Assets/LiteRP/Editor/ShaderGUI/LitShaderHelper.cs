using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    internal static class LitShaderHelper
    {
        public enum WorkflowMode
        {
            Specular = 0,
            Metallic
        }
        public enum SmoothnessMapChannel
        {
            MetallicAlpha,
            AlbedoAlpha,
        }
        
        internal static bool IsClearCoatEnabled(Material material)
        {
            return material.HasProperty(LiteRPShaderProperty.ClearCoat) && material.GetFloat(LiteRPShaderProperty.ClearCoat) > 0.0;
        }
        
        public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat(LiteRPShaderProperty.SmoothnessTextureChannel);
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;

            return SmoothnessMapChannel.MetallicAlpha;
        }
        
        // (shared by all lit shaders, including shadergraph Lit Target and Lit.shader)
        internal static void SetupSpecularWorkflowKeyword(Material material, out bool isSpecularWorkflow)
        {
            isSpecularWorkflow = false;     // default is metallic workflow
            if (material.HasProperty(LiteRPShaderProperty.WorkflowMode))
                isSpecularWorkflow = ((WorkflowMode)material.GetFloat(LiteRPShaderProperty.WorkflowMode)) == WorkflowMode.Specular;
            CoreUtils.SetKeyword(material, ShaderKeywordStrings.useSpecularWorkflow, isSpecularWorkflow);
        }
        
        public static void SetMaterialKeywords(Material material)
        {
            SetupSpecularWorkflowKeyword(material, out bool isSpecularWorkFlow);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            var specularGlossMap = isSpecularWorkFlow ? LiteRPShaderProperty.SpecGlossMap : LiteRPShaderProperty.MetallicGlossMap;
            var hasGlossMap = material.GetTexture(specularGlossMap) != null;

            CoreUtils.SetKeyword(material, ShaderKeywordStrings.MetallicSpecGlossMap, hasGlossMap);

            if (material.HasProperty(LiteRPShaderProperty.SpecularHighlights))
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.SpecularHighLightsOff,
                    material.GetFloat(LiteRPShaderProperty.SpecularHighlights) == 0.0f);
            if (material.HasProperty(LiteRPShaderProperty.EnvironmentReflections))
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.EnvironmentReflectionsOff,
                    material.GetFloat(LiteRPShaderProperty.EnvironmentReflections) == 0.0f);
            if (material.HasProperty(LiteRPShaderProperty.OptimizedBRDF))
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.OptimizedBRDFOff,
                    material.GetFloat(LiteRPShaderProperty.OptimizedBRDF) > 0.0f);
            if (material.HasProperty(LiteRPShaderProperty.OcclusionMap))
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.OcclusionMap, material.GetTexture(LiteRPShaderProperty.OcclusionMap));

            if (material.HasProperty(LiteRPShaderProperty.ParallaxMap))
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.ParallaxMap, material.GetTexture(LiteRPShaderProperty.ParallaxMap));

            if (material.HasProperty(LiteRPShaderProperty.SmoothnessTextureChannel))
            {
                var opaque = LiteRPShaderHelper.IsOpaque(material);
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.SmoothnessTextureAlbedoChannelA,
                    GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && opaque);
            }
            
            // Clear coat keywords are independent to remove possibility of invalid combinations.
            if (IsClearCoatEnabled(material))
            {
                var hasMap = material.HasProperty(LiteRPShaderProperty.ClearCoatMap) && material.GetTexture(LiteRPShaderProperty.ClearCoatMap) != null;
                if (hasMap)
                {
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoat, false);
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoatMap, true);
                }
                else
                {
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoat, true);
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoatMap, false);
                }
            }
            else
            {
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoat, false);
                CoreUtils.SetKeyword(material, ShaderKeywordStrings.ClearCoatMap, false);
            }
        }
    }
}