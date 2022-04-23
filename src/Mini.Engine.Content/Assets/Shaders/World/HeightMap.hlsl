#include "../Includes/Indexes.hlsl"
#include "Includes/SimplexNoise.hlsl"
#include "Includes/Utilities.hlsl"

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
    uint Count;
    uint Intervals;
};

struct Vertex
{
    float3 position;
    float2 texcoord;
    float3 normal;
};

RWStructuredBuffer<Vertex> Vertices : register(u0);
RWStructuredBuffer<int> Indices : register(u1);

RWTexture2D<float> MapHeight : register(u2);
RWTexture2D<float4> MapNormal : register(u3);

static const float2x2 m2 = float2x2(0.80, 0.60,
                            -0.60, 0.80);

float FBM(float2 coord)
{            
    float sum = 0.0f;
    
    float frequency = Frequency;    
    float amplitude = Amplitude;
    for (int i = 0; i < Octaves; i++)
    {        
        float noise = snoise(coord * frequency) * amplitude;
        frequency *= Lacunarity;
        amplitude *= Persistance;

        coord = Frequency * mul(m2, coord);
        
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
    // Maybe incorporate tricks from here? https://www.youtube.com/watch?v=BFld4EBO2RE
    
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
    
    float scale = 1.0f / Stride;
    
    float2 center = (float2(dispatchId.x, dispatchId.y) * scale) - float2(0.5f, 0.5f);
    
    float height = FBM(Offset + center);
    
    // Add an extra dramatic cliff effect for the heighest parts of the map
    height += (Amplitude * 0.55f) * smoothstep(Amplitude * 0.5f, Amplitude, height);
    
    float3 position = float3(center.x, height, center.y);

    MapHeight[dispatchId.xy] = position.y;
}
    
// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void NormalMapKernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
            
    float3 normal = ComputeNormalFromHeightMap(MapHeight, dispatchId.xy, Stride);

    MapNormal[dispatchId.xy] = float4(normal, 1.0f);
}

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void TriangulateKernel (in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Width || dispatchId.y >= Height)
    {
        return;
    }
    
    float scale = 1.0f / Width;
    float2 center = (float2(dispatchId.x, dispatchId.y) * scale) - float2(0.5f, 0.5f);
    
    
    uint2 textureIndex = uint2(dispatchId.x, dispatchId.y);
    float height = MapHeight[textureIndex] * 0.5f;
    float3 normal = MapNormal[textureIndex].xyz;
    float2 texcoord = float2(dispatchId.x, dispatchId.y) * scale;
        
    Vertex vertex;
    vertex.position = float3(center.x, height, center.y);
    vertex.normal = normal;
    vertex.texcoord = texcoord;
    
    uint index = ToOneDimensional(dispatchId.x, dispatchId.y, Stride);
    Vertices[index] = vertex;
}

#pragma ComputeShader
[numthreads(64, 1, 1)]
void IndicesKernel (in uint3 dispatchId : SV_DispatchThreadID)
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
                index = ToOneDimensional(x, y, Width);
                break;
            case 1:
                index = ToOneDimensional(x + 1, y, Width);
                break;
            case 2:
                index = ToOneDimensional(x + 1, y + 1, Width);
                break;
            case 3:
                index = ToOneDimensional(x + 1, y + 1, Width);
                break;
            case 4:
                index = ToOneDimensional(x, y + 1, Width);
                break;
            case 5:
                index = ToOneDimensional(x, y, Width);
                break;
        }
    }
    else
    {
        switch (remainder)
        {
            case 0:
                index = ToOneDimensional(x + 1, y, Width);
                break;
            case 1:
                index = ToOneDimensional(x + 1, y + 1, Width);
                break;
            case 2:
                index = ToOneDimensional(x, y + 1, Width);
                break;
            case 3:
                index = ToOneDimensional(x, y + 1, Width);
                break;
            case 4:
                index = ToOneDimensional(x, y, Width);
                break;
            case 5:
                index = ToOneDimensional(x + 1, y, Width);
                break;
        }
    }
    
    Indices[dispatchId.x] = (int) index;
}