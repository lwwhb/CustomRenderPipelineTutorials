#ifndef LITERP_DECLARE_DEPTH_TEXTURE_INCLUDED
#define LITERP_DECLARE_DEPTH_TEXTURE_INCLUDED

#include "SrpCoreShaderLibraryIncludes.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScalingClamping.hlsl"

TEXTURE2D_FLOAT(_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

float SampleSceneDepth(float2 uv, SAMPLER(samplerParam))
{
    uv = ClampAndScaleUVForBilinear(uv, _CameraDepthTexture_TexelSize.xy);
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, samplerParam, uv).r;
}

float SampleSceneDepth(float2 uv)
{
    return SampleSceneDepth(uv, sampler_PointClamp);
}

float LoadSceneDepth(uint2 pixelCoords)
{
    return LOAD_TEXTURE2D(_CameraDepthTexture, pixelCoords).r;
}
#endif