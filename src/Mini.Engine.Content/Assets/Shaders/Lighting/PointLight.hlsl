#include "../Includes/GBuffer.hlsl"
#include "../Includes/Coordinates.hlsl"
#include "Includes/Lights.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    noperspective float2 texcoord : TEXCOORD;
};
cbuffer Constants : register(b0)
{
    float4x4 InverseViewProjection;
    float3 CameraPosition;
    float __Padding;
};

cbuffer PerLightConstants : register(b1)
{
    float4x4 WorldViewProjection;
    float3 LightPosition;
    float Strength;
    float4 Color;
}

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Material : register(t1);
Texture2D Depth : register(t2);
Texture2D Normal : register(t3);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float4 position = float4(input.position.xyz, 1.0f);
    output.position = mul(WorldViewProjection, position);
    output.texcoord = ScreenToTexture(output.position.xy / output.position.w);

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_TARGET
{
    float3 albedo = ReadAlbedo(Albedo, TextureSampler, input.texcoord);
    float3 normal = ReadNormal(Normal, TextureSampler, input.texcoord);
    float3 position = ReadPosition(Depth, TextureSampler, input.texcoord, InverseViewProjection);
    Mat material = ReadMaterial(Material, TextureSampler, input.texcoord);

    float3 L = normalize(LightPosition - position);
    float3 Lo = ComputeLight(albedo, normal, material, position, CameraPosition, L, Color, Strength);
    Lo *= Attenuation(LightPosition.xyz, position);
    
    return float4(Lo, 1.0f);
}