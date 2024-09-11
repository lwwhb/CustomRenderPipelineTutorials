#ifndef LITERP_UNLIT_SURFACE_DATA_INCLUDED
#define LITERP_UNLIT_SURFACE_DATA_INCLUDED

#include "SurfaceFunctions.hlsl"

struct UnlitSurfaceData
{
    half3 albedo;
    half3 emission;
    half  alpha;
};

inline void InitializeUnlitSurfaceData(float2 uv, out UnlitSurfaceData outSurfaceData)
{
    outSurfaceData = (UnlitSurfaceData)0;
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);;
    outSurfaceData.albedo = AlphaModulate(albedoAlpha.rgb * _BaseColor.rgb, outSurfaceData.alpha);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));;
}

half4 LiteRPFragmentUnlit(UnlitSurfaceData surfaceData)
{
    return half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);
}

#endif