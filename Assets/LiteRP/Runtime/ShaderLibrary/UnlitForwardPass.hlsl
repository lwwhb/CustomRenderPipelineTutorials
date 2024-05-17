#ifndef LITERP_UNLIT_FORWARD_PASS_INCLUDED
#define LITERP_UNLIT_FORWARD_PASS_INCLUDED

#include "SrpCoreShaderLibraryIncludes.hlsl"
#include "Shadow.hlsl"
#include "UnlitInput.hlsl"
#include "SurfaceData.hlsl"
#include "SurfaceFunctions.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float fogCoord : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord             : TEXCOORD3;
    #endif
   
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    output.positionCS = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    
    #if defined(_FOG_FRAGMENT)
        output.fogCoord = vertexInput.positionVS.z;
    #else
        output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.positionWS = vertexInput.positionWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    
    return output;
}

void UnlitPassFragment(Varyings input, out half4 outColor : SV_Target0)
{
    UNITY_SETUP_INSTANCE_ID(input);
    half2 uv = input.uv;
    half4 texColor = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));

    half shadow = 1.0;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord = input.shadowCoord;
        half shadowFade = half(1.0);
        shadow = MainLightShadow(shadowCoord, input.positionWS);
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
        half shadowFade = GetMainLightShadowFade(input.positionWS);
        shadow = MainLightShadow(shadowCoord, input.positionWS);
    #else
        float4 shadowCoord = float4(0, 0, 0, 0);
        half shadowFade = half(1.0);
    #endif
    
    half3 color = texColor.rgb * _BaseColor.rgb * shadow;
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    half alpha = texColor.a * _BaseColor.a;
    alpha = AlphaDiscard(alpha, _Cutoff);
    color = AlphaModulate(color, alpha);

    UnlitSurfaceData surfaceData;
    InitializeUnlitSurfaceData(color, emission, alpha, surfaceData);

    half4 finalColor = LiteRPFragmentUnlit(surfaceData);

#if defined(_FOG_FRAGMENT)
#if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
    float viewZ = -input.fogCoord;
    float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
    half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
#else
    half fogFactor = 0;
#endif
#else
    half fogFactor = input.fogCoord;
#endif
    finalColor.rgb = MixFog(finalColor.rgb, fogFactor);
    finalColor.a = OutputAlpha(finalColor.a, IsSurfaceTypeTransparent(_Surface));

    outColor = finalColor;
}
#endif