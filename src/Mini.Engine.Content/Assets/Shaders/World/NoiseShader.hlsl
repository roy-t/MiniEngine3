static const uint NumThreads = 512;

cbuffer Constants : register(b0)
{    
    float2 Offset;
    float2 unused;
};

StructuredBuffer<float3> Tile : register(t0);
RWStructuredBuffer<float3> World : register(u1);

#pragma ComputeShader
[numthreads(NumThreads, 1, 1)]
void Kernel(in uint dispatchId : SV_DispatchThreadID)
{
    float3 input = Tile[dispatchId];
    World[dispatchId] = input + float3(0, 1, 0);
}