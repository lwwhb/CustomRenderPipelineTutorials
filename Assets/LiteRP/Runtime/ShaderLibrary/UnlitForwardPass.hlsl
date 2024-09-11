#ifndef LITERP_UNLIT_FORWARD_PASS_INCLUDED
#define LITERP_UNLIT_FORWARD_PASS_INCLUDED

#include "SrpCoreShaderLibraryIncludes.hlsl"
#include "Shadow.hlsl"
#include "UnlitInput.hlsl"
#include "UnlitSurfaceData.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    half fogCoord : TEXCOORD1;
    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        float3 positionWS               : TEXCOORD2;
    #endif
    
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
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    
    #if defined(_FOG_FRAGMENT)
        output.fogCoord = vertexInput.positionVS.z;
    #else
        output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    
    return output;
}

void UnlitPassFragment(Varyings input, out half4 outColor : SV_Target0)
{
    UNITY_SETUP_INSTANCE_ID(input);
    
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

    UnlitSurfaceData surfaceData;
    InitializeUnlitSurfaceData(input.uv, surfaceData);
    
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

    half4 color = LiteRPFragmentUnlit(surfaceData) * shadow;
    color.rgb = MixFog(color.rgb, fogFactor);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    outColor = color;
}
#endif