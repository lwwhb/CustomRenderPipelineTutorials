#ifndef LITERP_COPY_DEPTH_PASS_INCLUDED
#define LITERP_COPY_DEPTH_PASS_INCLUDED

#include "SrpCoreShaderLibraryIncludes.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D_FLOAT(_CameraDepthAttachment);
SAMPLER(sampler_CameraDepthAttachment);

float frag(Varyings input) : SV_Target
{
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthAttachment, sampler_CameraDepthAttachment, input.texcoord);
}

#endif
