#ifndef LITERP_SHADER_VARIABLES_FUNCTIONS_INCLUDED
#define LITERP_SHADER_VARIABLES_FUNCTIONS_INCLUDED

#include "ShaderVariablesInput.hlsl"
#include "ShaderInputData.hlsl"

#if UNITY_REVERSED_Z
    // TODO: workaround. There's a bug where SHADER_API_GL_CORE gets erroneously defined on switch.
    #if (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES3)
        //GL with reversed z => z clip range is [near, -far] -> remapping to [0, far]
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max((coord - _ProjectionParams.y)/(-_ProjectionParams.z-_ProjectionParams.y)*_ProjectionParams.z, 0)
    #else
        //D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
        //max is required to protect ourselves from near plane not being correct/meaningful in case of oblique matrices.
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((1.0-(coord)/_ProjectionParams.y)*_ProjectionParams.z),0)
    #endif
#elif UNITY_UV_STARTS_AT_TOP
    //D3d without reversed z => z clip range is [0, far] -> nothing to do
    #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
#else
    //Opengl => z clip range is [-near, far] -> remapping to [0, far]
    #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((coord + _ProjectionParams.y)/(_ProjectionParams.z+_ProjectionParams.y))*_ProjectionParams.z, 0)
#endif


VertexPositionInputs GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

// Constants that represent material surface types
//
// These are expected to align with the commonly used "_Surface" material property
static const half kSurfaceTypeOpaque = 0.0;
static const half kSurfaceTypeTransparent = 1.0;

// Returns true if the input value represents an opaque surface
bool IsSurfaceTypeOpaque(half surfaceType)
{
    return (surfaceType == kSurfaceTypeOpaque);
}

// Returns true if the input value represents a transparent surface
bool IsSurfaceTypeTransparent(half surfaceType)
{
    return (surfaceType == kSurfaceTypeTransparent);
}

// Only define the alpha clipping helpers when the alpha test define is present.
// This should help identify usage errors early.
#if defined(_ALPHATEST_ON)
// Returns true if AlphaToMask functionality is currently available
// NOTE: This does NOT guarantee that AlphaToMask is enabled for the current draw. It only indicates that AlphaToMask functionality COULD be enabled for it.
//       In cases where AlphaToMask COULD be enabled, we export a specialized alpha value from the shader.
//       When AlphaToMask is enabled:     The specialized alpha value is combined with the sample mask
//       When AlphaToMask is not enabled: The specialized alpha value is either written into the framebuffer or dropped entirely depending on the color write mask
bool IsAlphaToMaskAvailable()
{
    return (_AlphaToMaskAvailable != 0.0);
}

// When AlphaToMask is available:     Returns a modified alpha value that should be exported from the shader so it can be combined with the sample mask
// When AlphaToMask is not available: Terminates the current invocation if the alpha value is below the cutoff and returns the input alpha value otherwise
half AlphaClip(half alpha, half cutoff)
{
    // Produce 0.0 if the input value would be clipped by traditional alpha clipping and produce the original input value otherwise.
    // WORKAROUND: The alpha parameter in this ternary expression MUST be converted to a float in order to work around a known HLSL compiler bug.
    //             See Fogbugz 934464 for more information
    half clippedAlpha = (alpha >= cutoff) ? float(alpha) : 0.0;

    // Calculate a specialized alpha value that should be used when alpha-to-coverage is enabled

    // If the user has specified zero as the cutoff threshold, the expectation is that the shader will function as if alpha-clipping was disabled.
    // Ideally, the user should just turn off the alpha-clipping feature in this case, but in order to make this case work as expected, we force alpha
    // to 1.0 here to ensure that alpha-to-coverage never throws away samples when its active. (This would cause opaque objects to appear transparent)
    half alphaToCoverageAlpha = (cutoff <= 0.0) ? 1.0 : SharpenAlpha(alpha, cutoff);

    // When alpha-to-coverage is available:     Use the specialized value which will be exported from the shader and combined with the MSAA coverage mask.
    // When alpha-to-coverage is not available: Use the "clipped" value. A clipped value will always result in thread termination via the clip() logic below.
    alpha = IsAlphaToMaskAvailable() ? alphaToCoverageAlpha : clippedAlpha;

    // Terminate any threads that have an alpha value of 0.0 since we know they won't contribute anything to the final image
    clip(alpha - 0.0001);

    return alpha;
}
#endif

// Terminates the current invocation if the input alpha value is below the specified cutoff value and returns an updated alpha value otherwise.
// When provided, the offset value is added to the cutoff value during the comparison logic.
// The return value from this function should be exported as the final alpha value in fragment shaders so it can be combined with the MSAA coverage mask.
//
// When _ALPHATEST_ON is defined:     The returned value follows the behavior noted in the AlphaClip function
// When _ALPHATEST_ON is not defined: The returned value is equal to the original alpha input parameter
//
// NOTE: When _ALPHATEST_ON is not defined, this function is effectively a no-op.
real AlphaDiscard(real alpha, real cutoff, real offset = real(0.0))
{
#if defined(_ALPHATEST_ON)
    alpha = AlphaClip(alpha, cutoff + offset);
#endif

    return alpha;
}

half OutputAlpha(half alpha, bool isTransparent)
{
    if (isTransparent)
    {
        return alpha;
    }
    else
    {
#if defined(_ALPHATEST_ON)
        // Opaque materials should always export an alpha value of 1.0 unless alpha-to-coverage is available
        return IsAlphaToMaskAvailable() ? alpha : 1.0;
#else
        return 1.0;
#endif
    }
}

half3 AlphaModulate(half3 albedo, half alpha)
{
    // Fake alpha for multiply blend by lerping albedo towards 1 (white) using alpha.
    // Manual adjustment for "lighter" multiply effect (similar to "premultiplied alpha")
    // would be painting whiter pixels in the texture.
    // This emulates that procedure in shader, so it should be applied to the base/source color.
#if defined(_ALPHAMODULATE_ON)
    return lerp(half3(1.0, 1.0, 1.0), albedo, alpha);
#else
    return albedo;
#endif
}

half3 AlphaPremultiply(half3 albedo, half alpha)
{
    // Multiply alpha into albedo only for Preserve Specular material diffuse part.
    // Preserve Specular material (glass like) has different alpha for diffuse and specular lighting.
    // Logically this is "variable" Alpha blending.
    // (HW blend mode is premultiply, but with alpha multiply in shader.)
#if defined(_ALPHAPREMULTIPLY_ON)
    return albedo * alpha;
#endif
    return albedo;
}

//雾效
real ComputeFogFactorZ0ToFar(float z)
{
    #if defined(FOG_LINEAR)
    // factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
    float fogFactor = saturate(z * unity_FogParams.z + unity_FogParams.w);
    return real(fogFactor);
    #elif defined(FOG_EXP) || defined(FOG_EXP2)
    // factor = exp(-(density*z)^2)
    // -density * z computed at vertex
    return real(unity_FogParams.x * z);
    #else
        return real(0.0);
    #endif
}

real ComputeFogFactor(float zPositionCS)
{
    float clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(zPositionCS);
    return ComputeFogFactorZ0ToFar(clipZ_0Far);
}

half ComputeFogIntensity(half fogFactor)
{
    half fogIntensity = half(0.0);
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(FOG_EXP)
            // factor = exp(-density*z)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor));
        #elif defined(FOG_EXP2)
            // factor = exp(-(density*z)^2)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        #elif defined(FOG_LINEAR)
            fogIntensity = fogFactor;
        #endif
    #endif
    return fogIntensity;
}

// Force enable fog fragment shader evaluation
#define _FOG_FRAGMENT 1
real InitializeInputDataFog(float4 positionWS, real vertFogFactor)
{
    real fogFactor = 0.0;
#if defined(_FOG_FRAGMENT)
    #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        // Compiler eliminates unused math --> matrix.column_z * vec
        float viewZ = -(mul(UNITY_MATRIX_V, positionWS).z);
        // View Z is 0 at camera pos, remap 0 to near plane.
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
    #endif
#else
    fogFactor = vertFogFactor;
#endif
    return fogFactor;
}

float ComputeFogIntensity(float fogFactor)
{
    float fogIntensity = 0.0;
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(FOG_EXP)
            // factor = exp(-density*z)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor));
        #elif defined(FOG_EXP2)
            // factor = exp(-(density*z)^2)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        #elif defined(FOG_LINEAR)
            fogIntensity = fogFactor;
        #endif
    #endif
    return fogIntensity;
}

half3 MixFogColor(half3 fragColor, half3 fogColor, half fogFactor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    half fogIntensity = ComputeFogIntensity(fogFactor);
    fragColor = lerp(fogColor, fragColor, fogIntensity);
    #endif
    return fragColor;
}

float3 MixFogColor(float3 fragColor, float3 fogColor, float fogFactor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    float fogIntensity = ComputeFogIntensity(fogFactor);
    fragColor = lerp(fogColor, fragColor, fogIntensity);
    #endif
    return fragColor;
}

half3 MixFog(half3 fragColor, half fogFactor)
{
    return MixFogColor(fragColor, half3(unity_FogColor.rgb), fogFactor);
}

float3 MixFog(float3 fragColor, float fogFactor)
{
    return MixFogColor(fragColor, unity_FogColor.rgb, fogFactor);
}

#endif