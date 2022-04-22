#include "../Includes/Indexes.hlsl"
cbuffer ErosionConstants : register(b0)
{    
    uint Stride;
    float3 __Padding;
};

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapNormal : register(u1);
RWTexture2D<float4> MapVelocity : register(u2);


static const float SedimentCapacity = 0.05f;
static const float MinLocalTilt = 0.1f;

//float ComputeVelocity(uint2 index)
//{
//    float3 normal = MapNormal[index];
//    float3 direction = cross(float3(0, -1, 0), normal);
    
    
//}


//float ComputeTransportCapacity(uint2 index, float velocity)
//{
//    float3 normal = MapNormal[index];
    
//    // How angled the current surface is
//    float localTilt = max(MinLocalTilt, 1.0f - dot(normal, float3(0, 1, 0)));
    
//    // How much sediment we can transport based on a constant factor, the local tilt and velocity of the water
//    // http://www.nlpr.ia.ac.cn/2007papers/gjhy/gh116.pdf page 5
//    return SedimentCapacity * localTilt * length(velocity);
//}

// Run 8x8x1=64 threads per thread group, which means one full warp for AMD
// or two warps for NVIDIA. Leaving no threads idle.

#pragma ComputeShader
[numthreads(8, 8, 1)]
void Seed(in uint3 dispatchId : SV_DispatchThreadID)
{
     // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
    
    uint2 index = uint2(dispatchId.x, dispatchId.y);
    float3 normal = MapNormal[index].xyz;
        
    float localTilt = max(MinLocalTilt, 1.0f - dot(normal, float3(0, 1, 0)));
    
    float transportCapacity = SedimentCapacity * localTilt;
    
    MapHeight[index] = transportCapacity;
}

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
    
    float3 normal = MapNormal[index].xyz;    
    float4 velocity_volume = MapVelocity[index];
    float3 velocity = velocity_volume.xyz;
    float volume = velocity_volume.w;
    
    // How angled the current surface is
    float localTilt = max(MinLocalTilt, 1.0f - dot(normal, float3(0, 1, 0)));
    
    // How much sediment we can transport based on a constant factor, the local tilt and velocity of the water
    // http://www.nlpr.ia.ac.cn/2007papers/gjhy/gh116.pdf page 5
    float transportCapacity = SedimentCapacity * localTilt * length(velocity);

    // TODO: do not let this position get lower than any other point around it?
    
    // If we can transport more than the current volume, we erode. If we can transport less than the current volume we deposit
    MapHeight[index] += volume - transportCapacity;    
    //MapHeight[index] = transportCapacity;
    velocity_volume.w = transportCapacity;
            
    
    float3 sum = 0.0f;
    
    [unroll]
    for (uint i = 0; i < 8; i++)
    {
        uint x = i < 4 ? -1 : 1;
        uint y = i % 2 == 0 ? -1 : 1;
        
        float4 other = MapVelocity[index + uint2(x, y)];
        
        float incoming = max(0, dot(other.xyz, normalize(float3(x, 0, y))));
        
        float3 force = incoming * other.w * other.xyz;
        sum += force;
    }
        
    float3 down = cross(float3(0, -1, 0), normal);
    sum += down * transportCapacity;
    
    velocity_volume.xyz = sum / 9.0f;
    
    if (length(velocity_volume) == 0)
    {
        velocity_volume = float4(down.x, down.y, down.z, SedimentCapacity);
    }
    
    MapVelocity[index] = velocity_volume;
    
    
    // Steps
    // 1.   Given the current velocity (MapVelocity.xyz) and sediment volume (MapVelocity.w)
    //      increase or decrease the height of the current position. Making sure that the position
    //      does not get lower than any of the surrounding points (to avoid spikes)
    // 2.   Update the velocity and sediment volume based on the incoming and outgoing velocities
    //      I guess we should treat each neighbouring vector that points towards us as a force vector 
    //      and average them with a vector based on the slope of the current position?
    // 3.   PROFIT? Run this shader a few thousand times to get results?
    
}