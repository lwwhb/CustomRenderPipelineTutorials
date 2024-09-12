#ifndef LITERP_GLOBAL_ILLUMINATION_INCLUDED
#define LITERP_GLOBAL_ILLUMINATION_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "BRDF.hlsl"
#include "RealtimeLights.hlsl"

#define AMBIENT_PROBE_BUFFER 0
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"

// SH Vertex Evaluation. Depending on target SH sampling might be
// done completely per vertex or mixed with L2 term per vertex and L0, L1
// per pixel. See SampleSHPixel
half3 SampleSHVertex(half3 normalWS)
{
    #if defined(EVALUATE_SH_VERTEX)
        return EvaluateAmbientProbeSRGB(normalWS);
    #elif defined(EVALUATE_SH_MIXED)
        // no max since this is only L2 contribution
        return SHEvalLinearL2(normalWS, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
    #endif

    // Fully per-pixel. Nothing to compute.
    return half3(0.0, 0.0, 0.0);
}

// SH Pixel Evaluation. Depending on target SH sampling might be done
// mixed or fully in pixel. See SampleSHVertex
half3 SampleSHPixel(half3 L2Term, half3 normalWS)
{
    #if defined(EVALUATE_SH_VERTEX)
        return L2Term;
    #elif defined(EVALUATE_SH_MIXED)
        half3 res = L2Term + SHEvalLinearL0L1(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
        #ifdef UNITY_COLORSPACE_GAMMA
            res = LinearToSRGB(res);
        #endif
        return max(half3(0, 0, 0), res);
    #endif

    // Default: Evaluate SH fully per-pixel
    return EvaluateAmbientProbeSRGB(normalWS);
}

half3 SampleProbeSHVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, out float4 probeOcclusion)
{
    probeOcclusion = 1.0;

    #if (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
        return SampleProbeVolumeVertex(absolutePositionWS, normalWS, viewDir, probeOcclusion);
    #else
        return SampleSHVertex(normalWS);
    #endif
}

half3 SampleProbeSHVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir)
{
    float4 unusedProbeOcclusion = 0;
    return SampleProbeSHVertex(absolutePositionWS, normalWS, viewDir, unusedProbeOcclusion);
}

#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
    #ifdef USE_APV_PROBE_OCCLUSION
        #define SAMPLE_GI(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion) SampleProbeVolumePixel(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion)
    #else
        #define SAMPLE_GI(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion) SampleProbeVolumePixel(shName, absolutePositionWS, normalWS, viewDir, positionSS)
    #endif
#else
    #define SAMPLE_GI(shName, normalWSName) SampleSHPixel(shName, normalWSName)
#endif

#ifdef USE_APV_PROBE_OCCLUSION
    #define OUTPUT_SH4(absolutePositionWS, normalWS, viewDir, OUT, OUT_OCCLUSION) OUT.xyz = SampleProbeSHVertex(absolutePositionWS, normalWS, viewDir, OUT_OCCLUSION)
#else
    #define OUTPUT_SH4(absolutePositionWS, normalWS, viewDir, OUT, OUT_OCCLUSION) OUT.xyz = SampleProbeSHVertex(absolutePositionWS, normalWS, viewDir)
#endif

half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion)
{
    #if !defined(_ENVIRONMENTREFLECTIONS_OFF)
        half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_GlossyEnvironmentCubeMap, sampler_GlossyEnvironmentCubeMap, reflectVector, mip));
        half3 irradiance = DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR); 
        return irradiance * occlusion;
    #else
        return _GlossyEnvironmentColor.rgb * occlusion;
    #endif // _ENVIRONMENTREFLECTIONS_OFF
}

half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfDataClearCoat.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);
        // TODO: "grazing term" causes problems on full roughness
        half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

        // Blend with base layer using khronos glTF recommended way using NoV
        // Smooth surface & "ambiguous" lighting
        // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
        return (color * (1.0 - coatFresnel * clearCoatMask) + coatColor) * occlusion;
    #else
        return color * occlusion;
    #endif
}

#endif
