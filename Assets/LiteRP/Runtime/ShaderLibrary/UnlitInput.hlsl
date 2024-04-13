#ifndef LITERP_UNLIT_INPUT_INCLUDED
#define LITERP_UNLIT_INPUT_INCLUDED

float _AlphaToMaskAvailable;

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half3 _EmissionColor;
    half _Cutoff;
    half _Surface;
CBUFFER_END

#endif