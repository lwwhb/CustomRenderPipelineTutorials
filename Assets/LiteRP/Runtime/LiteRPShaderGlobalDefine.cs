using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class ShaderKeywordStrings
    {
        //Global Keyword String
        public const string MainLightShadows = "_MAIN_LIGHT_SHADOWS";                   //非Cascade阴影
        public const string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";    //Cascade阴影
        
        public const string SoftShadows = "_SHADOWS_SOFT";                              //使用软阴影
        public const string SoftShadowsLow = "_SHADOWS_SOFT_LOW";                       //使用软阴影-低质量
        public const string SoftShadowsMedium = "_SHADOWS_SOFT_MEDIUM";                 //使用软阴影-中质量
        public const string SoftShadowsHigh = "_SHADOWS_SOFT_HIGH";                     //使用软阴影-高质量

        public const string AdditionalLights = "_ADDITIONAL_LIGHTS";                    //使用辅助光源
        
        //Material Keyword String
        public const string AlphaTestOn = "_ALPHATEST_ON";                            //AlphaTest开启
        public const string AlphaPreMultiplyOn = "_ALPHAPREMULTIPLY_ON";              //Alpha预乘开启
        public const string AlphaModulateOn = "_ALPHAMODULATE_ON";                    //Alpha调制开启
        public const string SurfaceTypeTransparent = "_SURFACE_TYPE_TRANSPARENT";     //透明表面类型
        
        public const string useSpecularWorkflow = "_SPECULAR_SETUP";                   //使用高光工作流
        public const string SmoothnessTextureAlbedoChannelA = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";        //使用AlbedoMap的Alpha通道作为Smoothness
        public const string MetallicSpecGlossMap = "_METALLICSPECGLOSSMAP";            //使用GlossMap 
        public const string NormalMap = "_NORMALMAP";                                  //使用NormalMap
        public const string OcclusionMap = "_OCCLUSIONMAP";                            //使用OcclusionMap
        public const string ParallaxMap = "_PARALLAXMAP";                              //使用ParallaxMap
        public const string Emission = "_EMISSION";                                    //使用自发光
        public const string ClearCoat = "_CLEARCOAT";                                  //使用清漆层
        public const string ClearCoatMap = "_CLEARCOATMAP";                            //使用ClearCoatMap
        public const string ReceiveShadowsOff = "_RECEIVE_SHADOWS_OFF";                //接收阴影
        
        public const string SpecularHighLightsOff = "_SPECULARHIGHLIGHTS_OFF";              //不开启高光 
        public const string EnvironmentReflectionsOff = "_ENVIRONMENTREFLECTIONS_OFF";      //不开启环境反射
        public const string OptimizedBRDFOff = "_OPTIMIZED_BRDF_OFF";                       //不开启BRDF优化
        
        
    }

    internal static class ShaderGlobalKeywords
    {
        public static GlobalKeyword MainLightShadows;
        public static GlobalKeyword MainLightShadowCascades;
        
        public static GlobalKeyword SoftShadows;
        public static GlobalKeyword SoftShadowsLow;
        public static GlobalKeyword SoftShadowsMedium;
        public static GlobalKeyword SoftShadowsHigh;
        
        public static GlobalKeyword AdditionalLights;
        
        public static void InitializeShaderGlobalKeywords()
        {
            MainLightShadows = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadows);
            MainLightShadowCascades = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadowCascades);
            
            SoftShadows = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadows);
            SoftShadowsLow = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsLow);
            SoftShadowsMedium = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsMedium);
            SoftShadowsHigh = GlobalKeyword.Create(ShaderKeywordStrings.SoftShadowsHigh);

            AdditionalLights = GlobalKeyword.Create(ShaderKeywordStrings.AdditionalLights);
        }
    }

    internal static class ShaderPropertyName
    {
        public static readonly string time = "_Time";
        public static readonly string sinTime = "_SinTime";
        public static readonly string cosTime = "_CosTime";
        public static readonly string deltaTime = "unity_DeltaTime";
        public static readonly string timeParameters = "_TimeParameters";
        public static readonly string lastTimeParameters = "_LastTimeParameters";
        
        public static readonly string worldSpaceCameraPosName = "_WorldSpaceCameraPos";
        public static readonly string alphaToMaskAvailableName = "_AlphaToMaskAvailable";
        //public static readonly string scaledScreenParams = "_ScaledScreenParams";
        public static readonly string screenParams = "_ScreenParams";
        public static readonly string projectionParams = "_ProjectionParams";
        public static readonly string zBufferParams = "_ZBufferParams";
        public static readonly string orthoParams = "unity_OrthoParams";
        //public static readonly string globalMipBias = "_GlobalMipBias";
        
        // SetupLightsPass Const Buffer Begin
        public static readonly string mainLightPositionName = "_MainLightPosition";   
        public static readonly string mainLightColorName = "_MainLightColor";      

        public static readonly string additionalLightsCountName = "_AdditionalLightsCount";
        public static readonly string additionalLightsPositionName = "_AdditionalLightsPosition";
        public static readonly string additionalLightsColorName = "_AdditionalLightsColor";
        public static readonly string additionalLightsAttenuationName = "_AdditionalLightsAttenuation";
        public static readonly string additionalLightsSpotDirName = "_AdditionalLightsSpotDir";
        // SetupLightsPass Const Buffer End
        
        // MainLightShadowMapPass Begin
        public static readonly string lightDirectionName = "_LightDirection";
        public static readonly string lightPositionName = "_LightPosition";
        
        public static readonly string mainLightShadowmapName = "_MainLightShadowmapTexture";
        public static readonly string shadowBiasName = "_ShadowBias";
        
        // MainLightShadowMapPass Const Buffer Begin
        public static readonly string mainLightWorldToShadowName = "_MainLightWorldToShadow";
        public static readonly string mainLightShadowParamsName = "_MainLightShadowParams";
        public static readonly string mainLightCascadeShadowSplitSpheres0Name = "_CascadeShadowSplitSpheres0";
        public static readonly string mainLightCascadeShadowSplitSpheres1Name = "_CascadeShadowSplitSpheres1";
        public static readonly string mainLightCascadeShadowSplitSpheres2Name = "_CascadeShadowSplitSpheres2";
        public static readonly string mainLightCascadeShadowSplitSpheres3Name = "_CascadeShadowSplitSpheres3";
        public static readonly string mainLightCascadeShadowSplitSphereRadiiName = "_CascadeShadowSplitSphereRadii";
        public static readonly string mainLightShadowOffset0Name = "_MainLightShadowOffset0";
        public static readonly string mainLightShadowOffset1Name = "_MainLightShadowOffset1";
        public static readonly string mainLightShadowmapSizeName = "_MainLightShadowmapSize";
        // MainLightShadowMapPass Const Buffer Begin
        
        // MainLightShadowMapPass End
    }
    internal static class ShaderPropertyId
    {   
        // time
        public static readonly int time = Shader.PropertyToID(ShaderPropertyName.time);
        public static readonly int sinTime = Shader.PropertyToID(ShaderPropertyName.sinTime);
        public static readonly int cosTime = Shader.PropertyToID(ShaderPropertyName.cosTime);
        public static readonly int deltaTime = Shader.PropertyToID(ShaderPropertyName.deltaTime);
        public static readonly int timeParameters = Shader.PropertyToID(ShaderPropertyName.timeParameters);
        public static readonly int lastTimeParameters = Shader.PropertyToID(ShaderPropertyName.lastTimeParameters);
        
        //camera and screen params
        public static readonly int worldSpaceCameraPos = Shader.PropertyToID(ShaderPropertyName.worldSpaceCameraPosName);
        public static readonly int alphaToMaskAvailable = Shader.PropertyToID(ShaderPropertyName.alphaToMaskAvailableName);
        //public static readonly int scaledScreenParams = Shader.PropertyToID(ShaderPropertyName.scaledScreenParams);
        public static readonly int screenParams = Shader.PropertyToID(ShaderPropertyName.screenParams);
        public static readonly int projectionParams = Shader.PropertyToID(ShaderPropertyName.projectionParams);
        public static readonly int zBufferParams = Shader.PropertyToID(ShaderPropertyName.zBufferParams);
        public static readonly int orthoParams = Shader.PropertyToID(ShaderPropertyName.orthoParams);
        //public static readonly int globalMipBias = Shader.PropertyToID(ShaderPropertyName.globalMipBias);
        
        // SetupLightsPass Const Buffer Begin
        public static readonly int mainLightPosition = Shader.PropertyToID(ShaderPropertyName.mainLightPositionName);
        public static readonly int mainLightColor = Shader.PropertyToID(ShaderPropertyName.mainLightColorName);
        public static readonly int additionalLightsCount = Shader.PropertyToID(ShaderPropertyName.additionalLightsCountName);
        public static readonly int additionalLightsPosition = Shader.PropertyToID(ShaderPropertyName.additionalLightsPositionName);
        public static readonly int additionalLightsColor = Shader.PropertyToID(ShaderPropertyName.additionalLightsColorName);
        public static readonly int additionalLightsAttenuation = Shader.PropertyToID(ShaderPropertyName.additionalLightsAttenuationName);
        public static readonly int additionalLightsSpotDir = Shader.PropertyToID(ShaderPropertyName.additionalLightsSpotDirName);
        // SetupLightsPass Const Buffer End
        
        // MainLightShadowMapPass Begin
        
        public static readonly int lightDirection = Shader.PropertyToID(ShaderPropertyName.lightDirectionName);
        public static readonly int lightPosition = Shader.PropertyToID(ShaderPropertyName.lightPositionName);
        
        public static readonly int mainLightShadowmap = Shader.PropertyToID(ShaderPropertyName.mainLightShadowmapName);
        public static readonly int shadowBias = Shader.PropertyToID(ShaderPropertyName.shadowBiasName);
        
        // MainLightShadowMapPass Const Buffer Begin
        public static readonly int mainLightWorldToShadow = Shader.PropertyToID(ShaderPropertyName.mainLightWorldToShadowName);
        public static readonly int mainLightShadowParams = Shader.PropertyToID(ShaderPropertyName.mainLightShadowParamsName);
        public static readonly int mainLightCascadeShadowSplitSpheres0 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres0Name);
        public static readonly int mainLightCascadeShadowSplitSpheres1 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres1Name);
        public static readonly int mainLightCascadeShadowSplitSpheres2 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres2Name);
        public static readonly int mainLightCascadeShadowSplitSpheres3 = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSpheres3Name);
        public static readonly int mainLightCascadeShadowSplitSphereRadii = Shader.PropertyToID(ShaderPropertyName.mainLightCascadeShadowSplitSphereRadiiName);
        public static readonly int mainLightShadowOffset0 = Shader.PropertyToID(ShaderPropertyName.mainLightShadowOffset0Name);
        public static readonly int mainLightShadowOffset1 = Shader.PropertyToID(ShaderPropertyName.mainLightShadowOffset1Name);
        public static readonly int mainLightShadowmapSize = Shader.PropertyToID(ShaderPropertyName.mainLightShadowmapSizeName);
        // MainLightShadowMapPass Const Buffer End
        
        // MainLightShadowMapPass MainLightShadowMapPass End
        
        // glossy and ambient Begin
        public static readonly int glossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor");
        
        public static readonly int glossyEnvironmentCubeMap = Shader.PropertyToID("_GlossyEnvironmentCubeMap");
        public static readonly int glossyEnvironmentCubeMapHDR = Shader.PropertyToID("_GlossyEnvironmentCubeMap_HDR");

        public static readonly int ambientSkyColor = Shader.PropertyToID("unity_AmbientSky");
        public static readonly int ambientEquatorColor = Shader.PropertyToID("unity_AmbientEquator");
        public static readonly int ambientGroundColor = Shader.PropertyToID("unity_AmbientGround");
        
        public static readonly int shAr = Shader.PropertyToID("liteRP_SHAr");
        public static readonly int shAg = Shader.PropertyToID("liteRP_SHAg");
        public static readonly int shAb = Shader.PropertyToID("liteRP_SHAb");
        public static readonly int shBr = Shader.PropertyToID("liteRP_SHBr");
        public static readonly int shBg = Shader.PropertyToID("liteRP_SHBg");
        public static readonly int shBb = Shader.PropertyToID("liteRP_SHBb");
        public static readonly int shC = Shader.PropertyToID("liteRP_SHC");
        
        // glossy and ambient End
    }
}