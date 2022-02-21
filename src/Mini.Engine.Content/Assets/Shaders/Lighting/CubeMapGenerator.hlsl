#include "../Includes/Defines.hlsl"
#include "../Includes/Coordinates.hlsl"
#include "Includes/BRDF.hlsl"

static const float SampleDelta = 0.025f;

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : WORLD;
};

cbuffer Constants : register(b0)
{
    float4x4 InverseWorldViewProjection;
};

cbuffer EnvironmentConstants : register(b1)
{
    float Roughness;
    float3 Padding;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

// Similar to FullScreenTriangleVs but with an InverseViewProjection

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VERTEXID)
{
    PS_INPUT output;

    float4 pos = float4(vertexId == 1 ? 3.0f : -1.0f, vertexId == 2 ? 3.0f : -1.0f, 0.5f, 1.0f);

    output.position = pos;
    output.world = mul(InverseWorldViewProjection, pos).xyz;

    return output;
}

#pragma PixelShader
float4 AlbedoPs(PS_INPUT input) : SV_Target
{
    float2 uv = WorldToSpherical(normalize(input.world));
    return Texture.SampleLevel(TextureSampler, uv, 0.0f);
}

#pragma PixelShader
float4 IrradiancePs(PS_INPUT input) : SV_Target
{
    float3 normal = normalize(input.world);
    float3 irradiance = float3(0.0f, 0.0f, 0.0f);

    float3 right = normalize(cross(float3(0.0f, 1.0f, 0.0f), normal));
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
            float3 albedo = Texture.SampleLevel(TextureSampler, uv, 0.0f).rgb;
            irradiance += albedo;
            nrSamples++;
        }
    }

    return float4(PI * irradiance * (1.0f / nrSamples), 1.0f);
}

#pragma PixelShader
float4 EnvironmentPs(PS_INPUT input) : SV_Target
{
    float3 N = normalize(input.world);
    float3 R = N;
    float3 V = R;

    const uint SAMPLE_COUNT = 1024u;
    float totalWeight = 0.0f;
    float3 prefilteredColor = float3(0.0f, 0.0f, 0.0f);

    for (uint i = 0u; i < SAMPLE_COUNT; i++)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, Roughness);
        float3 L = normalize(2.0f * dot(V, H) * H - V);

        float NdotL = dot(N, L);
        if (NdotL > 0.0f)
        {
            // Compute the right mip-map level to sample from
            // to reduce the convolution artefacts
            float NdotH = max(dot(N, H), 0.0f);
            float HdotV = dot(H, V);
            float D = DistributionGGX(NdotH, Roughness);
            float pdf = (D * NdotH / (4.0f * HdotV)) + 0.0001f;

            float resolution = 512.0f; // resolution of source cubemap (per face)
            float saTexel = 4.0f * PI / (6.0f * resolution * resolution);
            float saSample = 1.0f / (float(SAMPLE_COUNT) * pdf + 0.0001f);

            float mipLevel = Roughness == 0.0f ? 0.0f : 0.5f * log2(saSample / saTexel);


            float2 uv = WorldToSpherical(L);
            float3 color = Texture.SampleLevel(TextureSampler, uv, mipLevel).rgb * NdotL;
            prefilteredColor += color;
            totalWeight += NdotL;
        }
    }
    prefilteredColor = prefilteredColor / totalWeight;

    return float4(prefilteredColor, 1.0f);
}