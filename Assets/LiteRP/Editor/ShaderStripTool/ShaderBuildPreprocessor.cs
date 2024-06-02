
using System;
using System.Collections.Generic;
using LiteRP.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    [Flags]
    public enum ShaderFeatures : long
    {
        None = 0,
        MainLight = (1L << 0),
        MainLightShadows = (1L << 1),
        MainLightShadowsCascade = (1L << 2),
        SoftShadows = (1L << 3),
        SoftShadowsLow = (1L << 4),
        SoftShadowsMedium = (1L << 5),
        SoftShadowsHigh = (1L << 6),
        ShadowsKeepOffVariants = (1L << 7),
        AlphaOutput = (1L << 8),
    }
    public class ShaderBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public static bool s_StripUnusedVariants;
        public static bool s_UseSoftShadowQualityLevelKeywords;
        public static bool s_StripDebugDisplayShaders;
        private static List<ShaderFeatures> s_SupportedFeaturesList = new();
        public static List<ShaderFeatures> supportedFeaturesList
        {
            get
            {
                // This can happen for example when building AssetBundles.
                if (s_SupportedFeaturesList.Count == 0)
                    GatherShaderFeatures(Debug.isDebugBuild);

                return s_SupportedFeaturesList;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
            GetGlobalAndPlatformSettings(isDevelopmentBuild);
        }
        public void OnPostprocessBuild(BuildReport report)
        {
        }
        
        private static void GatherShaderFeatures(bool isDevelopmentBuild)
        {
            GetGlobalAndPlatformSettings(isDevelopmentBuild);
        }
        
        private static void GetGlobalAndPlatformSettings(bool isDevelopmentBuild)
        {
            if (GraphicsSettings.TryGetRenderPipelineSettings<ShaderStrippingSetting>(out var shaderStrippingSettings))
                s_StripDebugDisplayShaders = !isDevelopmentBuild || shaderStrippingSettings.stripRuntimeDebugShaders;
            else
                s_StripDebugDisplayShaders = true;

            if (GraphicsSettings.TryGetRenderPipelineSettings<LiteRPShaderStrippingSetting>(out var liteRPShaderStrippingSettings))
            {
                s_StripUnusedVariants = liteRPShaderStrippingSettings.stripUnusedVariants;
            }
            
            s_UseSoftShadowQualityLevelKeywords = false;
        }
    }
}