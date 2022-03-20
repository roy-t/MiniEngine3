#include "../Includes/Indexes.hlsl"

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
    float input = Tile[index];
    World[index] = sin(dispatchId.x / 31.4f) * 30.0f;
}