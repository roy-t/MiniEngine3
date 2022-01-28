#include "Includes/Coordinates.hlsl"
#include "Includes/Defines.hlsl"

static const float SampleDelta = 0.025f;

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
    float3 normal = normalize(input.world);
    float3 irradiance = float3(0, 0, 0);

    float3 right = normalize(cross(float3(0, 1, 0), normal));
    float3 up = normalize(cross(normal, right));

    float nrSamples = 0.0f;

    // Sample a hemisphere by rotating completely around the azimuth of the sphere
    // and going up and down from the north pole to the equator via the zenith
    for (float azimuth = 0.0f; azimuth < TWO_PI; azimuth += SampleDelta)
    {
        for (float zenith = 0.0f; zenith < PI_OVER_TWO; zenith += SampleDelta)
        {
            float3 tangentSample = float3(sin(zenith) * cos(azimuth), sin(zenith) * sin(azimuth), cos(zenith));

            float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;

            float2 uv = WorldToSpherical(sampleVec);
            float3 albedo = Texture.SampleLevel(TextureSampler, uv, 0).rgb;            
            irradiance += albedo;
            nrSamples++;
        }
    }

    return float4(PI * irradiance * (1.0f / nrSamples), 1.0f);
}