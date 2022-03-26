#include "../Includes/Indexes.hlsl"
#include "Includes/SimplexNoise.hlsl"

cbuffer Constants : register(b0)
{    
    uint Stride;
    float2 Offset;
    float Amplitude;
    float Frequency;
    int Octaves;
    float Lacunarity;
    float Persistance;    
};

RWStructuredBuffer<float> World : register(u0);

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.
#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    int index = ToOneDimensional(dispatchId.x, dispatchId.y, Stride);

    float scale = 1.0f / Stride;

    float2 coord = Offset + float2(dispatchId.x, dispatchId.y) * scale;
    
    float sum = 0.0f;

    for (int i = 0; i < Octaves; i++)
    {
        float frequency = Frequency * pow(abs(Lacunarity), i);
        float amplitude = Amplitude * pow(abs(Persistance), i);
        float noise = snoise(coord * frequency) * amplitude;

        sum += noise;
    }

    World[index] = sum;
}