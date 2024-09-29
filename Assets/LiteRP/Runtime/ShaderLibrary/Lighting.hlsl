#ifndef LITERP_LIGHTING_INCLUDED
#define LITERP_LIGHTING_INCLUDED

#include "GlobalIllumination.hlsl"

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////
half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, float lightAttenuation, BRDFInputData brdfInputData,
    half clearCoatMask, bool specularHighlightsOff)
{
    half3 radiance = lightColor * (lightAttenuation * brdfInputData.NdotL);
    
    #if !defined(_OPTIMIZED_BRDF_OFF)
        half3 brdf = DirectBRDFDiffuseColor(brdfData, brdfInputData.NdotV, brdfInputData.NdotL, brdfInputData.LdotV);
    #else
        half3 brdf = brdfData.diffuse;
    #endif
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        #if !defined(_OPTIMIZED_BRDF_OFF)
            brdf += DirectBRDFSpecularColor(brdfData, brdfInputData.NdotH, brdfInputData.NdotL, brdfInputData.NdotV, brdfInputData.HdotV);
        #else
            brdf += brdfData.specular * DirectBRDFSpecular(brdfData, brdfInputData.NdotH, brdfInputData.HdotV); //HdotV == LdotH
        #endif
    }
    
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
    #if !defined(_OPTIMIZED_BRDF_OFF)
        half3 brdfCoat = DirectBRDFSpecularColor(brdfDataClearCoat, brdfInputData.NdotH, brdfInputData.NdotL, brdfInputData.NdotV, brdfInputData.HdotV);
    #else
        half3 brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, brdfInputData.NdotH, brdfInputData.HdotV);
    #endif

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - brdfInputData.NdotV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
    #endif // _CLEARCOAT
#endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, BRDFInputData brdfInputData, half clearCoatMask, bool specularHighlightsOff)
{
    return LightingPhysicallyBased(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, brdfInputData, clearCoatMask, specularHighlightsOff);
}


half3 LightingPhysicallyBased(BRDFData brdfData, Light light, BRDFInputData brdfInputData, bool specularHighlightsOff)
{
    const BRDFData noClearCoat = (BRDFData)0;
    return LightingPhysicallyBased(brdfData, noClearCoat, light, brdfInputData, 0.0, specularHighlightsOff);
}

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 emissionColor;
};

half3 CalculateLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;
    lightingColor += lightingData.giColor;
    lightingColor += lightingData.mainLightColor;
    lightingColor += lightingData.additionalLightsColor;
    lightingColor *= albedo;
    lightingColor += lightingData.emissionColor;
    return lightingColor;
}

half4 CalculateFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = CalculateLightingColor(lightingData, 1);

    return half4(finalColor, alpha);
}

half4 CalculateFinalColor(LightingData lightingData, half3 albedo, half alpha, float fogCoord)
{
    #if defined(_FOG_FRAGMENT)
        #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
            float viewZ = -fogCoord;
            float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
            half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
        #else
            half fogFactor = 0;
        #endif
    #else
        half fogFactor = fogCoord;
    #endif
    half3 lightingColor = CalculateLightingColor(lightingData, albedo);
    half3 finalColor = MixFog(lightingColor, fogFactor);

    return half4(finalColor, alpha);
}

LightingData CreateLightingData(InputData inputData, LitSurfaceData surfaceData)
{
    LightingData lightingData;

    lightingData.giColor = inputData.bakedGI;
    lightingData.emissionColor = surfaceData.emission;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
/// PBR lighting...
////////////////////////////////////////////////////////////////////////////////
half4 LiteRPFragmentPBR(InputData inputData, LitSurfaceData surfaceData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
        bool specularHighlightsOff = true;
    #else
        bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    
    Light mainLight = GetMainLight(inputData);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    BRDFInputData brdfInputData;
    InitializeBRDFInputData(mainLight.direction, inputData.normalWS, inputData.viewDirectionWS, brdfInputData);
    
    // lwwhb  ao临时为1 clearCoatMask为0
    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, 0.0,
                            inputData.bakedGI, 1.0, inputData.positionWS, brdfInputData);

    lightingData.mainLightColor = LightingPhysicallyBased(brdfData, brdfDataClearCoat, mainLight, brdfInputData, surfaceData.clearCoatMask, specularHighlightsOff);
    
    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, aoFactor);
    
    lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, light,
        brdfInputData, specularHighlightsOff);
    LIGHT_LOOP_END
    #endif

    #if REAL_IS_HALF
        // Clamp any half.inf+ to HALF_MAX
        return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
    #else
        return CalculateFinalColor(lightingData, surfaceData.alpha);
    #endif
}

#endif
