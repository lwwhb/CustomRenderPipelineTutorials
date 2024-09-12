#ifndef LITERP_UNITY_VARIABLES_INPUT_INCLUDED
#define LITERP_UNITY_VARIABLES_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define UNITY_LIGHTMODEL_AMBIENT (glstate_lightmodel_ambient * 2)

// ----------------------------------------------------------------------------

// Time (t = time since current level load) values from Unity
float4 _Time; // (t/20, t, t*2, t*3)
float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)
float4 _CosTime; // cos(t/8), cos(t/4), cos(t/2), cos(t)
float4 unity_DeltaTime; // dt, 1/dt, smoothdt, 1/smoothdt
float4 _TimeParameters; // t, sin(t), cos(t)
float4 _LastTimeParameters; // t, sin(t), cos(t)

float3 _WorldSpaceCameraPos;

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

// ----------------------------------------------------------------------------

#ifndef DOTS_INSTANCING_ON // UnityPerDraw cbuffer doesn't exist with hybrid renderer

// Block Layout should be respected due to SRP Batcher
CBUFFER_START(UnityPerDraw)
    // Space block Feature
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    float4 unity_LODFade; // x is the fade value ranging within [0,1]. y is x quantized into 16 levels
    real4 unity_WorldTransformParams; // w is usually 1.0, or -1.0 for odd-negative scale transforms

    // Render Layer block feature
    // Only the first channel (x) contains valid data and the float must be reinterpreted using asuint() to extract the original 32 bits values.
    float4 unity_RenderingLayer;

    // Light Indices block feature
    // These are set internally by the engine upon request by RendererConfiguration.
    half4 unity_LightData;
    half4 unity_LightIndices[2];

    float4 unity_ProbesOcclusion;

    // SH block feature
    real4 unity_SHAr;
    real4 unity_SHAg;
    real4 unity_SHAb;
    real4 unity_SHBr;
    real4 unity_SHBg;
    real4 unity_SHBb;
    real4 unity_SHC;

    // Renderer bounding box.
    float4 unity_RendererBounds_Min;
    float4 unity_RendererBounds_Max;

    // Velocity
    float4x4 unity_MatrixPreviousM;
    float4x4 unity_MatrixPreviousMI;
    //X : Use last frame positions (right now skinned meshes are the only objects that use this
    //Y : Force No Motion
    //Z : Z bias value
    //W : Camera only
    float4 unity_MotionVectorsParams;

    // Sprite.
    float4 unity_SpriteColor;
    //X : FlipX
    //Y : FlipY
    //Z : Reserved for future use.
    //W : Reserved for future use.
    float4 unity_SpriteProps;
CBUFFER_END

#endif // UNITY_DOTS_INSTANCING_ENABLED


float4x4 glstate_matrix_transpose_modelview0;

// ----------------------------------------------------------------------------

real4 glstate_lightmodel_ambient;
real4 unity_AmbientSky;
real4 unity_AmbientEquator;
real4 unity_AmbientGround;
real4 unity_IndirectSpecColor;
float4 unity_FogParams;
real4  unity_FogColor;

float4x4 glstate_matrix_projection;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_MatrixInvP;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixInvVP;

real4 unity_ShadowColor;

// ----------------------------------------------------------------------------

// Unity specific
TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);
TEXTURECUBE(unity_SpecCube1);
SAMPLER(samplerunity_SpecCube1);

// ----------------------------------------------------------------------------

// TODO: all affine matrices should be 3x4.
// TODO: sort these vars by the frequency of use (descending), and put commonly used vars together.
// Note: please use UNITY_MATRIX_X macros instead of referencing matrix variables directly.

float4x4 _PrevViewProjMatrix;         // non-jittered. Motion vectors.
float4x4 _NonJitteredViewProjMatrix;  // non-jittered.
float4x4 _ViewProjMatrix; // TODO: URP currently uses unity_MatrixVP, see Input.hlsl

float4x4 _ViewMatrix;
float4x4 _ProjMatrix;
float4x4 _InvViewProjMatrix;
float4x4 _InvViewMatrix;
float4x4 _InvProjMatrix;
float4   _InvProjParam;
float4   _ScreenSize;       // {w, h, 1/w, 1/h}
float4   _FrustumPlanes[6]; // {(a, b, c) = N, d = -dot(N, P)} [L, R, T, B, N, F]

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

#endif //LITERP_UNITY_VARIABLES_INPUT_INCLUDED