using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    internal class ShaderScriptableStripper : IShaderVariantStripper, IShaderVariantStripperScope
    {
        internal struct StrippingDataContext
        {
            public Shader shader { get; set; }
            public ShaderFeatures shaderFeatures { get; set; }
            public ShaderSnippetData snippetData { get; set; }
            public ShaderCompilerData compilerData { get; set; }
            
            public ShaderCompilerPlatform shaderCompilerPlatform { get => compilerData.shaderCompilerPlatform; set {} }
            public bool isGLDevice { get => compilerData.shaderCompilerPlatform == ShaderCompilerPlatform.GLES3x || compilerData.shaderCompilerPlatform == ShaderCompilerPlatform.OpenGLCore; set{} }
            
            
            public ShaderType shaderType { get => snippetData.shaderType; set{} }
            public string passName { get => snippetData.passName; set {} }
            public PassType passType { get => snippetData.passType; set {} }
            public PassIdentifier passIdentifier { get => snippetData.pass; set {} }
            
            public bool stripSoftShadowQualityLevels { get; set; }
            public bool stripUnusedVariants { get; set; }
            public bool IsHDRDisplaySupportEnabled { get; set; }
            public bool IsHDRShaderVariantValid { get => HDROutputUtils.IsShaderVariantValid(compilerData.shaderKeywordSet, PlayerSettings.allowHDRDisplaySupport); set { } }
            public bool IsKeywordEnabled(LocalKeyword keyword)
            {
                return compilerData.shaderKeywordSet.IsEnabled(keyword);
            }
            public bool IsShaderFeatureEnabled(ShaderFeatures feature)
            {
                return (shaderFeatures & feature) != 0;
            }
            public bool PassHasKeyword(LocalKeyword keyword)
            {
                return ShaderUtil.PassHasKeyword(shader, snippetData.pass, keyword, snippetData.shaderType, shaderCompilerPlatform);
            }
        }
        
        // Keywords
        LocalKeyword m_MainLightShadows;
        LocalKeyword m_MainLightShadowsCascades;
        LocalKeyword m_SoftShadows;
        LocalKeyword m_SoftShadowsLow;
        LocalKeyword m_SoftShadowsMedium;
        LocalKeyword m_SoftShadowsHigh;
        LocalKeyword m_AlphaTestOn;
        
        public bool active => LiteRenderPipeline.asset != null;
        
        public bool CanRemoveVariant([DisallowNull] Shader shader, ShaderSnippetData shaderVariant, ShaderCompilerData shaderCompilerData)
        {
            StrippingDataContext strippingDataContext = new StrippingDataContext()
            {
                stripSoftShadowQualityLevels = !ShaderBuildPreprocessor.s_UseSoftShadowQualityLevelKeywords,
                stripUnusedVariants = ShaderBuildPreprocessor.s_StripUnusedVariants,
                IsHDRDisplaySupportEnabled = PlayerSettings.allowHDRDisplaySupport,
                shader = shader,
                snippetData = shaderVariant,
                compilerData = shaderCompilerData
            };

            // All feature sets need to have this variant unused to be stripped out.
            bool removeInput = true;
            for (var index = 0; index < ShaderBuildPreprocessor.supportedFeaturesList.Count; index++)
            {
                strippingDataContext.shaderFeatures = ShaderBuildPreprocessor.supportedFeaturesList[index];
                //剔除没用的Shader
                if (StripUnusedShaders(ref strippingDataContext))
                    continue;
                //剔除没用的Shader Pass
                if (StripUnusedPass(ref strippingDataContext))
                    continue;
                //剔除不正确的Shader Variant
                if (StripInvalidVariants(ref strippingDataContext))
                    continue;
                //剔除不支持的Shader Variant
                if (StripUnsupportedVariants(ref strippingDataContext))
                    continue;
                //剔除没用的的管线功能
                if (StripUnusedFeatures(ref strippingDataContext))
                    continue;

                removeInput = false;
                break;
            }
            return removeInput;
        }
        
        public void BeforeShaderStripping(Shader shader)
        {
            if (shader != null)
                InitializeLocalShaderKeywords(shader);
        }

        public void AfterShaderStripping(Shader shader)
        {
        }
        
        private LocalKeyword TryGetLocalKeyword(Shader shader, string name)
        {
            return shader.keywordSpace.FindKeyword(name);
        }
        private void InitializeLocalShaderKeywords([DisallowNull] Shader shader)
        {
            m_MainLightShadows = TryGetLocalKeyword(shader, ShaderKeywordStrings.MainLightShadows);
            m_MainLightShadowsCascades = TryGetLocalKeyword(shader, ShaderKeywordStrings.MainLightShadowCascades);
            m_SoftShadows = TryGetLocalKeyword(shader, ShaderKeywordStrings.SoftShadows);
            m_SoftShadowsLow = TryGetLocalKeyword(shader, ShaderKeywordStrings.SoftShadowsLow);
            m_SoftShadowsMedium = TryGetLocalKeyword(shader, ShaderKeywordStrings.SoftShadowsMedium);
            m_SoftShadowsHigh = TryGetLocalKeyword(shader, ShaderKeywordStrings.SoftShadowsHigh);
            m_AlphaTestOn = TryGetLocalKeyword(shader, ShaderKeywordStrings.AlphaTestOn);
        }
        private bool StripUnusedShaders(ref StrippingDataContext strippingDataContext)
        {
            if (!strippingDataContext.stripUnusedVariants)
                return false;
            return false;
        }
        private bool StripUnusedPass(ref StrippingDataContext strippingDataContext)
        {
            //检查ShadowCaster Pass
            if (strippingDataContext.passType == PassType.ShadowCaster)
            {
                if (!strippingDataContext.IsShaderFeatureEnabled(ShaderFeatures.MainLightShadows))
                    return true;
            }

            return false;
        }
        
        private bool StripInvalidVariants(ref StrippingDataContext strippingDataContext)
        {
            // 剔除主光源阴影Shader变体
            bool isMainShadowNoCascades = strippingDataContext.IsKeywordEnabled(m_MainLightShadows);
            bool isMainShadowCascades = strippingDataContext.IsKeywordEnabled(m_MainLightShadowsCascades);
            bool isMainShadow = isMainShadowNoCascades || isMainShadowCascades;
            bool isShadowVariant = isMainShadow;
            if (!isShadowVariant && (strippingDataContext.IsKeywordEnabled(m_SoftShadows) ||
                                     strippingDataContext.IsKeywordEnabled(m_SoftShadowsLow) ||
                                     strippingDataContext.IsKeywordEnabled(m_SoftShadowsMedium)
                                     || strippingDataContext.IsKeywordEnabled(m_SoftShadowsHigh)))
                return true;
            return false;
        }
        
        internal bool StripUnsupportedVariants(ref StrippingDataContext strippingDataContext)
        {
            return false;
        }
        
        internal bool StripUnusedFeatures(ref StrippingDataContext strippingDataContext)
        {
            return false;
        }
    }
}