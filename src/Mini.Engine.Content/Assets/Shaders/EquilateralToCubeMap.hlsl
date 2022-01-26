#include "Includes/Coordinates.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : WORLD;    
};

cbuffer Constants : register(b0)
{
    float4x4 InverseWorldViewProjection;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    output.position = float4(input.position, 1.0f);
    output.world = mul(InverseWorldViewProjection, float4(input.position, 1.0f)).xyz;

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    float2 uv = WorldToSpherical(normalize(input.world));
    return Texture.SampleLevel(TextureSampler, uv, 0);    
}