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
    half3   vertexLighting;
    half3   bakedGI;
    float2  normalizedScreenSpaceUV;
    half4   shadowMask;
    half3x3 tangentToWorld;
};

// Time (t = time since current level load) values from Unity
float4 _Time; // (t/20, t, t*2, t*3)
float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)
float4 _CosTime; // cos(t/8), cos(t/4), cos(t/2), cos(t)
float4 unity_DeltaTime; // dt, 1/dt, smoothdt, 1/smoothdt
float4 _TimeParameters; // t, sin(t), cos(t)
float4 _LastTimeParameters; // t, sin(t), cos(t)

float3 _WorldSpaceCameraPos;
float _AlphaToMaskAvailable;

float4 _ScaledScreenParams;

// x = 1 or -1 (-1 if projection is flipped)
// y = near plane
// z = far plane
// w = 1/far plane
float4 _ProjectionParams;

// x = width
// y = height
// z = 1 + 1.0/width
// w = 1 + 1.0/height
float4 _ScreenParams;

// Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
// x = 1-far/near
// y = far/near
// z = x/far
// w = y/far
// or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
// x = -1+far/near
// y = 1
// z = x/far
// w = 1/far
float4 _ZBufferParams;

// x = orthographic camera's width
// y = orthographic camera's height
// z = unused
// w = 1.0 if camera is ortho, 0.0 if perspective
float4 unity_OrthoParams;

// x = Mip Bias
// y = 2.0 ^ [Mip Bias]
float2 _GlobalMipBias;

// scaleBias.x = flipSign
// scaleBias.y = scale
// scaleBias.z = bias
// scaleBias.w = unused
uniform float4 _ScaleBias;
uniform float4 _ScaleBiasRt;

// { w / RTHandle.maxWidth, h / RTHandle.maxHeight } : xy = currFrame, zw = prevFrame
uniform float4 _RTHandleScale;

float4 unity_CameraWorldClipPlanes[6];
// Projection matrices of the camera. Note that this might be different from projection matrix
// that is set right now, e.g. while rendering shadows the matrices below are still the projection
// of original camera.
float4x4 unity_CameraProjection;
float4x4 unity_CameraInvProjection;
float4x4 unity_WorldToCamera;
float4x4 unity_CameraToWorld;

// Light ----------------------------------------------------------------------------
float4 _MainLightPosition;
half4 _MainLightColor;

half4 _AdditionalLightsCount;

#ifndef DOTS_INSTANCING_ON // UnityPerDraw cbuffer doesn't exist with hybrid renderer
    // Block Layout should be respected due to SRP Batcher
    CBUFFER_START(UnityPerDraw)
        // Space block Feature
        float4x4 unity_ObjectToWorld;
        float4x4 unity_WorldToObject;
        real4 unity_WorldTransformParams; // w is usually 1.0, or -1.0 for odd-negative scale transforms

        // Render Layer block feature
        // Only the first channel (x) contains valid data and the float must be reinterpreted using asuint() to extract the original 32 bits values.
        float4 unity_RenderingLayer;

        // Light Indices block feature
        // These are set internally by the engine upon request by RendererConfiguration.
        half4 unity_LightData;
        half4 unity_LightIndices[2];

        // Reflection Probe 0 block feature
        // HDR environment map decode instructions
        real4 unity_SpecCube0_HDR;
        //real4 unity_SpecCube1_HDR;    //lwwhb

        float4 unity_SpecCube0_BoxMax;          // w contains the blend distance
        float4 unity_SpecCube0_BoxMin;          // w contains the lerp value
        float4 unity_SpecCube0_ProbePosition;   // w is set to 1 for box projection

        // SH block feature
        real4 unity_SHAr;
        real4 unity_SHAg;
        real4 unity_SHAb;
        real4 unity_SHBr;
        real4 unity_SHBg;
        real4 unity_SHBb;
        real4 unity_SHC;

        // Velocity
        float4x4 unity_MatrixPreviousM;
        float4x4 unity_MatrixPreviousMI;
    CBUFFER_END
#endif // UNITY_DOTS_INSTANCING_ENABLED

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

// ----------------------------------------------------------------------------
real4 glstate_lightmodel_ambient;
real4 unity_AmbientSky;
real4 unity_AmbientEquator;
real4 unity_AmbientGround;
real4 unity_IndirectSpecColor;

float4 unity_FogParams;
real4  unity_FogColor;

half4 _GlossyEnvironmentColor;
//lwwhb
//half4 _GlossyEnvironmentCubeMap_HDR;

// ----------------------------------------------------------------------------

// Unity specific
TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);
TEXTURECUBE(unity_SpecCube1);
SAMPLER(samplerunity_SpecCube1);
//lwwhb
//TEXTURECUBE(_GlossyEnvironmentCubeMap);
//SAMPLER(sampler_GlossyEnvironmentCubeMap);

// ----------------------------------------------------------------------------
float4x4 glstate_matrix_projection;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;  
float4x4 unity_MatrixInvP;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixInvVP;

float4x4 OptimizeProjectionMatrix(float4x4 M)
{
    // Matrix format (x = non-constant value).
    // Orthographic Perspective  Combined(OR)
    // | x 0 0 x |  | x 0 x 0 |  | x 0 x x |
    // | 0 x 0 x |  | 0 x x 0 |  | 0 x x x |
    // | x x x x |  | x x x x |  | x x x x | <- oblique projection row
    // | 0 0 0 1 |  | 0 0 x 0 |  | 0 0 x x |
    // Notice that some values are always 0.
    // We can avoid loading and doing math with constants.
    M._21_41 = 0;
    M._12_42 = 0;
    return M;
}

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

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#endif