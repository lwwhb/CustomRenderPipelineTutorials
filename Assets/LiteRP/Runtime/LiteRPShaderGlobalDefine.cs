using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class ShaderKeywordStrings
    {
        public const string MainLightShadows = "_MAIN_LIGHT_SHADOWS";                   //非Cascade阴影
        public const string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";    //Cascade阴影
        
        public const string _ALPHATEST_ON = "_ALPHATEST_ON";                            //AlphaTest开启
        public const string _ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";              //Alpha预乘开启
        public const string _ALPHAMODULATE_ON = "_ALPHAMODULATE_ON";                    //Alpha调制开启
        public const string _SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";    //透明表面类型
        
                        
        public const string _NORMALMAP = "_NORMALMAP";                                  //使用Normal
        public const string _EMISSION = "_EMISSION";                                    //使用自发光
        public const string _RECEIVE_SHADOWS_OFF = "_RECEIVE_SHADOWS_OFF";              //接收阴影
        
        public const string SoftShadows = "_SHADOWS_SOFT";                              //使用软阴影
        public const string SoftShadowsLow = "_SHADOWS_SOFT_LOW";                       //使用软阴影-低质量
        public const string SoftShadowsMedium = "_SHADOWS_SOFT_MEDIUM";                 //使用软阴影-中质量
        public const string SoftShadowsHigh = "_SHADOWS_SOFT_HIGH";                     //使用软阴影-高质量
    }

    internal static class ShaderGlobalKeywords
    {
        public static GlobalKeyword MainLightShadows;
        public static GlobalKeyword MainLightShadowCascades;
        
        public static GlobalKeyword SoftShadows;
        public static GlobalKeyword SoftShadowsLow;
        public static GlobalKeyword SoftShadowsMedium;
        public static GlobalKeyword SoftShadowsHigh;
        
        public static void InitializeShaderGlobalKeywords()
        {
            ShaderGlobalKeywords.MainLightShadows = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadows);
            ShaderGlobalKeywords.MainLightShadowCascades = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadowCascades);
            ShaderGlobalKeywords.SoftShadows = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadows);
            ShaderGlobalKeywords.SoftShadowsLow = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsLow);
            ShaderGlobalKeywords.SoftShadowsMedium = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsMedium);
            ShaderGlobalKeywords.SoftShadowsHigh = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsHigh);
        }
    }

    internal static class ShaderPropertyName
    {
        public static readonly string worldSpaceCameraPosName = "_WorldSpaceCameraPos";
        
        public static readonly string alphaToMaskAvailableName = "_AlphaToMaskAvailable";
        
        public static readonly string shadowBiasName = "_ShadowBias";
        public static readonly string lightDirectionName = "_LightDirection";
        public static readonly string lightPositionName = "_LightPosition";
        
        // MainLightShadowMapPass Begin
        public static readonly string mainLightShadowmapName = "_MainLightShadowmapTexture";
        
        // MainLightShadowMapPass Const Buffer Begin
        public static readonly string mainLightWorldToShadowName = "_MainLightWorldToShadow";
        public static readonly string mainLightShadowParamsName = "_MainLightShadowParams";
        public static readonly string mainLightCascadeShadowSplitSpheres0Name = "_CascadeShadowSplitSpheres0";
        public static readonly string mainLightCascadeShadowSplitSpheres1Name = "_CascadeShadowSplitSpheres1";
        public static readonly string mainLightCascadeShadowSplitSpheres2Name = "_CascadeShadowSplitSpheres2";
        public static readonly string mainLightCascadeShadowSplitSpheres3Name = "_CascadeShadowSplitSpheres3";
        public static readonly string mainLightCascadeShadowSplitSphereRadiiName = "_CascadeShadowSplitSphereRadii";
        public static readonly string mainLightShadowOffset0 = "_MainLightShadowOffset0";
        public static readonly string mainLightShadowOffset1 = "_MainLightShadowOffset1";
        public static readonly string mainLightShadowmapSize = "_MainLightShadowmapSize";
        // MainLightShadowMapPass Const Buffer Begin
        
        // MainLightShadowMapPass End
    }
    internal static class ShaderPropertyId
    {
        public static readonly int worldSpaceCameraPos = Shader.PropertyToID(ShaderPropertyName.worldSpaceCameraPosName);
        
        public static readonly int alphaToMaskAvailable = Shader.PropertyToID(ShaderPropertyName.alphaToMaskAvailableName);
        
        public static readonly int shadowBias = Shader.PropertyToID(ShaderPropertyName.shadowBiasName);
        public static readonly int lightDirection = Shader.PropertyToID(ShaderPropertyName.lightDirectionName);
        public static readonly int lightPosition = Shader.PropertyToID(ShaderPropertyName.lightPositionName);
        
        // MainLightShadowMapPass Begin
        public static readonly int mainLightShadowmap = Shader.PropertyToID(ShaderPropertyName.mainLightShadowmapName);
        
        // MainLightShadowMapPass Const Buffer Begin
        public static readonly int mainLightWorldToShadow = Shader.PropertyToID(ShaderPropertyName.mainLightWorldToShadowName);
        public static readonly int mainLightShadowParams = Shader.PropertyToID(ShaderPropertyName.mainLightShadowParamsName);
        public static readonly int mainLightCascadeShadowSplitSpheres0 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres0Name);
        public static readonly int mainLightCascadeShadowSplitSpheres1 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres1Name);
        public static readonly int mainLightCascadeShadowSplitSpheres2 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres2Name);
        public static readonly int mainLightCascadeShadowSplitSpheres3 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres3Name);
        public static readonly int mainLightCascadeShadowSplitSphereRadii = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSphereRadiiName);
        public static readonly int mainLightShadowOffset0 = Shader.PropertyToID(ShaderPropertyName.mainLightShadowOffset0);
        public static readonly int mainLightShadowOffset1 = Shader.PropertyToID(ShaderPropertyName.mainLightShadowOffset1);
        public static readonly int mainLightShadowmapSize = Shader.PropertyToID(ShaderPropertyName.mainLightShadowmapSize);
        // MainLightShadowMapPass Const Buffer End
        
        // MainLightShadowMapPass MainLightShadowMapPass End
    }
}