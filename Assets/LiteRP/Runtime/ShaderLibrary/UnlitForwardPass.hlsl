#ifndef LITERP_UNLIT_FORWARD_PASS_INCLUDED
#define LITERP_UNLIT_FORWARD_PASS_INCLUDED

#include "SrpCoreShaderLibraryIncludes.hlsl"
#include "UnlitInput.hlsl"
#include "ShaderVariablesFunctions.hlsl"
#include "SurfaceData.hlsl"


struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float fogCoord : TEXCOORD1;
    float4 positionCS : SV_POSITION;
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    output.positionCS = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #if defined(_FOG_FRAGMENT)
    output.fogCoord = vertexInput.positionVS.z;
    #else
    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    return output;
}

void UnlitPassFragment(Varyings input, out half4 outColor : SV_Target0)
{
    half2 uv = input.uv;
    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half3 color = texColor.rgb * _BaseColor.rgb;
    #if defined(_EMISSION)
    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb;
    #else
    half3 emission = half3(0, 0, 0);
    #endif
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