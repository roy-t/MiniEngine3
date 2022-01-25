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
    float4x4 WorldViewProjection;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);


#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    
    float4 position = float4(input.position, 1.0f);
    output.position = mul(WorldViewProjection, position);
    output.world = input.position;

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{   
    float2 uv = WorldToSpherical(normalize(input.world));    
    return Texture.Sample(TextureSampler, uv);
}