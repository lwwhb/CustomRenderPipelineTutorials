using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    internal static class LitShaderHelper
    {
        public enum SmoothnessMapChannel
        {
            MetallicAlpha,
            AlbedoAlpha,
        }
        internal static bool IsOpaque(Material material)
        {
            bool opaque = true;
            if (material.HasProperty(LiteRPShaderProperty.SurfaceType))
                opaque = ((LiteRPShaderGUI.SurfaceType)material.GetFloat(LiteRPShaderProperty.SurfaceType) == LiteRPShaderGUI.SurfaceType.Opaque);
            return opaque;
        }
        
        public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;

            return SmoothnessMapChannel.MetallicAlpha;
        }
        
        public static void SetMaterialKeywords(Material material)
        {
            var hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
            CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);

            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
                    material.GetFloat("_SpecularHighlights") == 0.0f);
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                    material.GetFloat("_EnvironmentReflections") == 0.0f);
            if (material.HasProperty("_OcclusionMap"))
                CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));

            if (material.HasProperty("_ParallaxMap"))
                CoreUtils.SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                var opaque = IsOpaque(material);
                CoreUtils.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
                    GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && opaque);
            }
        }
    }
}