#include "Includes/Coordinates.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : POSITION;
    float2 texcoord : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
};

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    
    output.position = float4(input.position, 1.0f);
    output.texcoord = input.texcoord;
    output.world = input.position + float3(0, 0, 1);

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{    
    float3 world = mul(WorldViewProjection, float4(input.world, 1.0f)).xyz; 
    float2 uv = WorldToSpherical(normalize(world));
    return Texture.Sample(TextureSampler, uv);
}