#ifndef LITERP_SURFACE_DATA_INCLUDED
#define LITERP_SURFACE_DATA_INCLUDED

struct UnlitSurfaceData
{
    half3 albedo;
    half3 emission;
    half  alpha;
};

void InitializeUnlitSurfaceData(half3 albedo, half3 emission, half alpha, out UnlitSurfaceData surfaceData)
{
    surfaceData = (UnlitSurfaceData)0;
    surfaceData.albedo = albedo;
    surfaceData.emission = emission;
    surfaceData.alpha = alpha;
}

half4 LiteRPFragmentUnlit(UnlitSurfaceData surfaceData)
{
    return half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);
}

#endif