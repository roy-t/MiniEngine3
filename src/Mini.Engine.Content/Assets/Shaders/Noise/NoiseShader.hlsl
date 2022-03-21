#include "../Includes/Indexes.hlsl"
#include "Includes/SimplexNoise.hlsl"

cbuffer Constants : register(b0)
{
    uint Stride;    
    float3 unused;
};

StructuredBuffer<float> Tile : register(t0);
RWStructuredBuffer<float> World : register(u1);

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    int index = ToOneDimensional(dispatchId.x, dispatchId.y, Stride);
    //float input = Tile[index];

    float2 coord = float2(dispatchId.x, dispatchId.y);

    float high = snoise(coord / 30.0f) * 1;
    float medium = snoise(coord / 100.0f) * 10;
    float low = snoise(coord / 500.0f) * 100;

    World[index] = low + medium + high; //low + medium + high;
}