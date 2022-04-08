#include "../Includes/Indexes.hlsl"
cbuffer ErosionConstants : register(b0)
{    
    uint Stride;
    float3 __Padding;
};

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapNormal : register(u1);

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
    
    uint2 index = uint2(dispatchId.x, dispatchId.y);
    MapHeight[index] = 0;
    //NoiseMapNormal[index] = float4(normal, 1.0f);
}