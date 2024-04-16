#ifndef LITERP_SHADER_INPUT_DATA_INCLUDED
#define LITERP_SHADER_INPUT_DATA_INCLUDED

struct VertexPositionInputs
{
    float3 positionWS;      // World space position
    float3 positionVS;      // View space position
    float4 positionCS;      // Homogeneous clip space position
    float4 positionNDC;     // Homogeneous normalized device coordinates
};

struct VertexNormalInputs
{
    real3 tangentWS;        // World space tangent
    real3 bitangentWS;      // World space bitangent
    float3 normalWS;        // World space normal
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;

TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);

TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);

#endif 