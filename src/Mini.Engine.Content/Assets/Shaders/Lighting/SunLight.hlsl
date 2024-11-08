﻿#include "../Includes/GBuffer.hlsl"
#include "Includes/Lights.hlsl"
#include "Includes/Shadows.hlsl"

// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

cbuffer Constants : register(b0)
{
    float4 Color;
    float3 SurfaceToLight;
    float Strength;
    float4x4 InverseViewProjection;
    float3 CameraPosition;    
    ShadowProperties Shadow;
}

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Material : register(t1);
Texture2D Depth : register(t2);
Texture2D Normal : register(t3);


Texture2DArray ShadowMap : register(t4);
SamplerComparisonState ShadowSampler : register(s1);

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_TARGET
{
    float3 albedo = ReadAlbedo(Albedo, TextureSampler, input.tex);
    float3 normal = ReadNormal(Normal, TextureSampler, input.tex);
    float3 position = ReadPosition(Depth, TextureSampler, input.tex, InverseViewProjection);
    
    Mat material = ReadMaterial(Material, TextureSampler, input.tex);  
    float depth = distance(position, CameraPosition);

    float lightFactor = ComputeLightFactorPCF(position, depth, Shadow, ShadowMap, ShadowSampler);
    float3 Lo = float3(0.0f, 0.0f, 0.0f);

    if (lightFactor > 0)
    {
        // No attenuation since sunlight has already crossed an extreme distance
        Lo = ComputeLight(albedo, normal, material, position, CameraPosition, SurfaceToLight, Color, Strength);
    }    

    return float4(Lo * lightFactor, 1.0f);
}