#ifndef LITERP_SHADER_VARIABLES_INPUT_INCLUDED
#define LITERP_SHADER_VARIABLES_INPUT_INCLUDED

#define MAX_VISIBLE_LIGHT_COUNT_LOW_END_MOBILE (16)
#define MAX_VISIBLE_LIGHT_COUNT_MOBILE (32)
#define MAX_VISIBLE_LIGHT_COUNT_DESKTOP (256)

#if defined(SHADER_API_MOBILE) && defined(SHADER_API_GLES30)
    #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_LOW_END_MOBILE
// WebGPU's minimal limits are based on mobile rather than desktop, so it will need to assume mobile.
#elif defined(SHADER_API_MOBILE) || (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES3) || defined(SHADER_API_WEBGPU) // Workaround because SHADER_API_GLCORE is also defined when SHADER_API_SWITCH is
    #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_MOBILE
#else
    #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_DESKTOP
#endif

struct InputData
{
    float3  positionWS;
    float4  positionCS;
    float3  normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half    fogCoord;
    half3   bakedGI;
    float2  normalizedScreenSpaceUV;
    half3x3 tangentToWorld;
};

///////////////////////////////////////////////////////////////////////////////
//                      Constant Buffers                                     //
///////////////////////////////////////////////////////////////////////////////

half4 _GlossyEnvironmentColor;

half4 _GlossyEnvironmentCubeMap_HDR;
TEXTURECUBE(_GlossyEnvironmentCubeMap);
SAMPLER(sampler_GlossyEnvironmentCubeMap);

float _AlphaToMaskAvailable;
float4 _ScaledScreenParams;

// Light ----------------------------------------------------------------------------
float4 _MainLightPosition;
half4 _MainLightColor;
half4 _AdditionalLightsCount;

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    StructuredBuffer<LightData> _AdditionalLightsBuffer;
    StructuredBuffer<int> _AdditionalLightsIndices;
#else
    // GLES3 causes a performance regression in some devices when using CBUFFER.
    #ifndef SHADER_API_GLES3
        CBUFFER_START(AdditionalLights)
    #endif
        float4 _AdditionalLightsPosition[MAX_VISIBLE_LIGHTS];
        // In Forward+, .a stores whether the light is using subtractive mixed mode.
        half4 _AdditionalLightsColor[MAX_VISIBLE_LIGHTS];
        half4 _AdditionalLightsAttenuation[MAX_VISIBLE_LIGHTS];
        half4 _AdditionalLightsSpotDir[MAX_VISIBLE_LIGHTS];
        half4 _AdditionalLightsOcclusionProbes[MAX_VISIBLE_LIGHTS];
    #ifndef SHADER_API_GLES3
        CBUFFER_END
    #endif
#endif

#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     OptimizeProjectionMatrix(glstate_matrix_projection)
#define UNITY_MATRIX_I_P   unity_MatrixInvP
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_I_VP  unity_MatrixInvVP
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
#define UNITY_PREV_MATRIX_M   unity_MatrixPreviousM
#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI

#include "UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "DOTSInstancing.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#endif //LITERP_SHADER_VARIABLES_INPUT_INCLUDED