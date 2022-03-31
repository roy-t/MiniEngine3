#include "../Includes/Indexes.hlsl"
#include "Includes/SimplexNoise.hlsl"

cbuffer NoiseConstants : register(b0)
{    
    uint Stride;
    float2 Offset;
    float Amplitude;
    float Frequency;
    int Octaves;
    float Lacunarity;
    float Persistance;    
};

cbuffer TriangulateConstants : register(b1)
{
    uint Width;
    uint Height;
    float2 Unused;
};

cbuffer IndicesConstants : register(b2)
{
    uint NWidth;
    uint NHeight;
    uint Count;    
    uint Intervals;
};
    
struct Vertex
{
    float3 position;
    float2 texcoord;
    float3 normal;
};

RWStructuredBuffer<Vertex> HeightMap : register(u0);
RWStructuredBuffer<int> Indices : register(u1);

RWTexture2D<float> NoiseMapHeight : register(u2);
RWTexture2D<float4> NoiseMapNormal : register(u3);

float Noise(float2 coord)
{
    float sum = 0.0f;
    for (int i = 0; i < Octaves; i++)
    {
        float frequency = Frequency * pow(abs(Lacunarity), i);
        float amplitude = Amplitude * pow(abs(Persistance), i);
        float noise = snoise(coord * frequency) * amplitude;

        sum += noise;
    }
    
    return sum;
}

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void NoiseMapKernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
    
    float scale = 1.0f / Stride;
    
    float2 center = (float2(dispatchId.x, dispatchId.y) * scale) - float2(0.5f, 0.5f);
    float2 west = center + (float2(-1.0f, 0.0f) * scale);
    float2 north = center + (float2(0.0f, -1.0f) * scale);
    float2 east = center + (float2(1.0f, 0.0f) * scale);
    float2 south = center + (float2(0.0f, 1.0f) * scale);
    
    float3 vWest = float3(west.x, Noise(Offset + west), west.y);
    float3 vNorth = float3(north.x, Noise(Offset + north), north.y);
    float3 vEast = float3(east.x, Noise(Offset + east), east.y);
    float3 vSouth = float3(south.x, Noise(Offset + south), south.y);
    
    float3 position = (vWest + vNorth + vEast + vSouth) / 4.0f;
    
    float3 wXn = normalize(cross(position - vNorth, position - vWest));
    float3 eXS = normalize(cross(position - vSouth, position - vEast));
    
    float3 normal = normalize((wXn + eXS) / 2.0f);
    
    uint2 index = uint2(dispatchId.x, dispatchId.y);
    NoiseMapHeight[index] = position.y;
    NoiseMapNormal[index] = float4(normal, 1.0f);
}

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void TriangulateKernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Width || dispatchId.y >= Height)
    {
        return;
    }
    
    float scale = 1.0f / Width;
    float2 center = (float2(dispatchId.x, dispatchId.y) * scale) - float2(0.5f, 0.5f);
    
    
    uint2 textureIndex = uint2(dispatchId.x, dispatchId.y);
    float height = NoiseMapHeight[textureIndex];
    float3 normal = NoiseMapNormal[textureIndex].xyz;
    float2 texcoord = float2(dispatchId.x, dispatchId.y) * scale;
        
    Vertex vertex;
    vertex.position = float3(center.x, height, center.y);
    vertex.normal = normal;
    vertex.texcoord = texcoord;
    
    uint index = ToOneDimensional(dispatchId.x, dispatchId.y, Stride);            
    HeightMap[index] = vertex;
}


#pragma ComputeShader
[numthreads(64, 1, 1)]
void IndicesKernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Count)
    {
        return;
    }
        
    uint2 position = ToTwoDimensional(dispatchId.x / 6, Intervals);
   
    uint x = position.x;
    uint y = position.y;
    
    uint remainder = dispatchId.x % 6;
    
    uint index = 0;
    
    if ((x % 2 == 0) == (y % 2 == 0))
    {
        switch (remainder)
        {
            case 0:
                index = ToOneDimensional(x, y, NWidth);
                break;
            case 1:
                index = ToOneDimensional(x + 1, y, NWidth);
                break;
            case 2:
                index = ToOneDimensional(x + 1, y + 1, NWidth);
                break;
            case 3:
                index = ToOneDimensional(x + 1, y + 1, NWidth);
                break;
            case 4:
                index = ToOneDimensional(x, y + 1, NWidth);
                break;
            case 5:
                index = ToOneDimensional(x, y, NWidth);
                break;
        }
    }
    else
    {
        switch (remainder)
        {
            case 0:
                index = ToOneDimensional(x + 1, y, NWidth);
                break;
            case 1:
                index = ToOneDimensional(x + 1, y + 1, NWidth);
                break;
            case 2:
                index = ToOneDimensional(x, y + 1, NWidth);
                break;
            case 3:
                index = ToOneDimensional(x, y + 1, NWidth);
                break;
            case 4:
                index = ToOneDimensional(x, y, NWidth);
                break;
            case 5:
                index = ToOneDimensional(x + 1, y, NWidth);
                break;
        }
    }
    
    Indices[dispatchId.x] = (int) index;
}