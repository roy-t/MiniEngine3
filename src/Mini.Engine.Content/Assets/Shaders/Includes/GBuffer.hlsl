#ifndef __GBUFFER
#define __GBUFFER

#include "Normals.hlsl"
#include "Material.hlsl"
#include "Coordinates.hlsl"

// Reads properties stored in a gbuffer, should be sampled using a LinearClamp sampler

float3 ReadAlbedo(Texture2D albedo, sampler samp, float2 texcoord)
{
    return albedo.Sample(samp, texcoord).rgb;
}

float3 ReadNormal(Texture2D normal, sampler samp, float2 texcoord)
{
    float3 N = normal.Sample(samp, texcoord).xyz;
    return UnpackNormal(N);
}

Mat ReadMaterial(Texture2D material, sampler samp, float2 texcoord)
{
    Mat mat;

    float4 m = material.Sample(samp, texcoord);

    mat.Metalicness = m.x;
    mat.Roughness = m.y;
    mat.AmbientOcclusion = m.z;

    return mat;
}

float ReadDepth(Texture2D depth, sampler samp, float2 texcoord)
{
    return depth.Sample(samp, texcoord).r;
}

float3 ComputePosition(float2 texcoord, float depth, float4x4 inverseViewProjection)
{
    // Compute screen-space position
    float2 screen = TextureToScreen(texcoord);
    
    float4 position;
    position.x = screen.x;
    position.y = screen.y;
    position.z = depth;
    position.w = 1.0f;

    // Transform to world space
    position = mul(position, inverseViewProjection);
    position /= position.w;

    return position.xyz;
}

float3 ReadPosition(Texture2D depth, sampler samp, float2 texcoord, float4x4 inverseViewProjection)
{
    float d = ReadDepth(depth, samp, texcoord);
    return ComputePosition(texcoord, d, inverseViewProjection);
}

#endif
