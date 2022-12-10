#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Coordinates.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 previousPosition : POSITION0;
    float4 currentPosition : POSITION1;
    float3 world :  WORLD;
    float2 texcoord : TEXCOORD;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float4 normal : SV_Target2;
    float2 velocity : SV_Target3;
};

cbuffer Constants : register(b0)
{
    float4x4 PreviousWorldViewProjection;
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float2 PreviousJitter;
    float2 Jitter;
    float3 DepositionColor;
    float3 ErosionColor;
    float ErosionMultiplier;
};

sampler TextureSampler : register(s0);
Texture2D Erosion : register(t0);
Texture2D Normal : register(t1);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3)World;
    float4 position = float4(input.position, 1.0f);

    output.position = mul(WorldViewProjection, position);
    output.previousPosition = mul(PreviousWorldViewProjection, position);
    output.currentPosition = output.position;
    output.world = mul(World, position).xyz;
    output.texcoord = input.texcoord;

    return output;
}

// static const float4 DepositionColor = float4(88.0f, 102.0f, 37.0f, 255.0f) / 255.0f;
// static const float4 ErosionColor = float4(178.0f, 160.0f, 112.0f, 255.0f) / 255.0f;

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;

    float erosion = (Erosion.Sample(TextureSampler, input.texcoord).r * ErosionMultiplier) + 0.65f;
    output.albedo = ToLinear(float4(lerp(ErosionColor, DepositionColor, erosion), 1.0f));

    float3 normal = Normal.Sample(TextureSampler, input.texcoord).xyz;
    output.normal = float4(PackNormal(normal), 1.0f);
    
    float metalicness = 0.0f;
    float roughness = 1.0f;
    float ambientOcclusion = 1.0f;
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);

    input.previousPosition /= input.previousPosition.w;
    input.currentPosition /= input.currentPosition.w;
    float2 previousUv = ScreenToTexture(input.previousPosition.xy - PreviousJitter);
    float2 currentUv = ScreenToTexture(input.currentPosition.xy - Jitter);
    
    output.velocity = previousUv - currentUv;

    return output;
}