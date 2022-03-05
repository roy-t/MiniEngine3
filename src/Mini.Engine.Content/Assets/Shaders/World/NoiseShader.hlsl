#include "../Includes/Indexes.hlsl"

cbuffer Constants : register(b0)
{
    uint Stride;    
    float3 unused;
};

StructuredBuffer<float3> Tile : register(t0);
RWStructuredBuffer<float3> World : register(u1);

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    int index = ToOneDimensional(dispatchId.x, dispatchId.y, Stride);
    float3 input = Tile[index];
    World[index] = input + float3(index, dispatchId.x, dispatchId.y);
}