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
Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);

Texture2D HeigthMapNormal : register(t5);
Texture2D Erosion : register(t6);

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

struct MultiUv
{
    float4 albedo;
    float metalicness;
    float roughness;
    float ambientOcclusion;
    float3 normal;
};


MultiUv SampleTextures(float3 world, float2 texCoord, float erosion, float3 heightMapNormal)
{
    float4 albedo = Albedo.Sample(TextureSampler, texCoord);
    float metalicness = Metalicness.Sample(TextureSampler, texCoord).r;
    float roughness = Roughness.Sample(TextureSampler, texCoord).r;
    float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, texCoord).r;

    float4 tint = ToLinear(float4(lerp(ErosionColor, DepositionColor, erosion), 1.0f));

    float3 V = normalize(CameraPosition - world);
    float3 normal = PerturbNormal(Normal, TextureSampler, heightMapNormal, V, texCoord);

    MultiUv output;
    output.albedo = lerp(albedo, tint, 0.75f);
    output.metalicness = metalicness;
    output.roughness = roughness;
    output.ambientOcclusion = ambientOcclusion;
    output.normal = normal;

    return output;
}

MultiUv MixMultiUv(MultiUv one, MultiUv two)
{
    MultiUv output;
    output.albedo = one.albedo + two.albedo;
    output.metalicness = one.metalicness + two.metalicness;
    output.roughness = one.roughness + two.roughness;
    output.ambientOcclusion = one.ambientOcclusion + two.ambientOcclusion;
    output.normal = one.normal + two.normal;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;

    float erosion = (Erosion.Sample(TextureSampler, input.texcoord).r * ErosionMultiplier) + 0.65f;

    float3 heightMapNormal = HeigthMapNormal.Sample(TextureSampler, input.texcoord).xyz;

    float2 layerOneTexcoord = input.texcoord * 83.0f;
    MultiUv layerOne = SampleTextures(input.world, layerOneTexcoord, erosion, heightMapNormal);

    float2 r2 = float2(sin(0.33f), cos(0.33f));
    float2 layerTwoTexcoord = (r2 *input.texcoord) * 53.0f;
    MultiUv layerTwo = SampleTextures(input.world, layerTwoTexcoord, erosion, heightMapNormal);
    

    MultiUv sums = MixMultiUv(layerOne, layerTwo);

    float count = 2;

    float4 albedo = sums.albedo / count;
    float metalicness = sums.metalicness / count;
    float roughness = sums.roughness / count;
    float ambientOcclusion = sums.ambientOcclusion / count;
    float3 normal = normalize(sums.normal / count);
    
    output.albedo = albedo;
    output.normal = float4(PackNormal(normal), 1.0f);
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);

    input.previousPosition /= input.previousPosition.w;
    input.currentPosition /= input.currentPosition.w;
    float2 previousUv = ScreenToTexture(input.previousPosition.xy - PreviousJitter);
    float2 currentUv = ScreenToTexture(input.currentPosition.xy - Jitter);
    
    output.velocity = previousUv - currentUv;

    return output;
}