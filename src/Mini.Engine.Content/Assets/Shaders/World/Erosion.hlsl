#include "../Includes/Indexes.hlsl"
#include "Includes/Utilities.hlsl"

cbuffer ErosionConstants : register(b0)
{    
    uint Stride;
    float3 __Padding;
};

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapVelocityIn : register(u1);
RWTexture2D<float4> MapVelocityOut : register(u2);
RWTexture2D<float4> MapMassIn : register(u3);
RWTexture2D<float4> MapMassOut : register(u4);

static const float SedimentCapacity = 0.001f;
static const float MinLocalTilt = 0.1f;

static const float Gravity = 9.81;
static const float3 Up = float3(0.0f, 1.0f, 0.0f);
static const float3 Down = float3(0.0f, -1.0f, 0.0f);

float ComputeLocalTilt(float3 normal)
{
    return max(MinLocalTilt, 1.0f - dot(normal, Up));
}

float3 ComputeFlowDirection(float3 normal)
{
    return cross(Down, normal);
}

float ComputeTransportCapacity(float3 normal)
{
    const float force = 1.0f;
    float localTilt = ComputeLocalTilt(normal);
    
    return SedimentCapacity * localTilt * force;
}

// Start with 1 unit of water on every pixel
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
    
    float3 normal = ComputeNormalFromHeightMap(MapHeight, dispatchId.xy, Stride);
    float3 flowDirection = ComputeFlowDirection(normal);
    float water = SedimentCapacity * MinLocalTilt;
    float mass = ComputeTransportCapacity(normal);
    MapVelocityOut[index] = float4(flowDirection, 0.0f);
    MapMassOut[index] = float4(water, mass, 0.0f, 0.0f);
}


#pragma ComputeShader
[numthreads(8, 8, 1)]
void Erode(in uint3 dispatchId : SV_DispatchThreadID)
{
    // When not using a power of two input we might be out-of-bounds
    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
    {
        return;
    }
    
    uint2 index = uint2(dispatchId.x, dispatchId.y);            
    
    // Check how much water and mass has flowed to our tile
    float water = MapMassIn[index].x;
    float absorbedSediment = MapMassIn[index].y;
                        
    // Check how much mass we can transport
    float3 normal = ComputeNormalFromHeightMap(MapHeight, index, Stride);
    float transportCapacity = ComputeTransportCapacity(normal) * water;
        
    // Erode or deposit based on how much mass the water transports away from here
    if (transportCapacity < absorbedSediment)
    {
        float deposit = absorbedSediment - transportCapacity;
        MapHeight[index] += deposit;
    }
    
    if (transportCapacity > absorbedSediment)
    {
        // TODO: don't erode further than neighbours
        float erosion = transportCapacity - absorbedSediment;
        MapHeight[index] -= erosion;
    }
    
    // Now that all water, and the mass in it has flown away, update with how much we get from our neighbours
    water = 0;
    absorbedSediment = 0;
    
    [unroll]
    for (uint i = 0; i < 8; i++)
    {
        uint x = i < 4 ? -1 : 1;
        uint y = i % 2 == 0 ? -1 : 1;
        
        uint2 position = index + uint2(x, y);
        
        // Compute how much of the water/abasored mass on the neighbouring tile is going to flow our way
        float3 velocityIn = MapVelocityIn[position].xyz;
        float factor = max(0, dot(normalize(float3(velocityIn.x, 0, velocityIn.z)), normalize(-float3(x, 0, y))));
        
        // Update our water and mass values
        water += factor * MapMassIn[position].x;
        absorbedSediment += factor * MapMassIn[position].y;
    }       
    
    float3 flowDirection = ComputeFlowDirection(normal);
    
    MapVelocityOut[index] = float4(flowDirection, 0);
    MapMassOut[index] = float4(water, absorbedSediment, 0, 0);
}

//#pragma ComputeShader
//[numthreads(8, 8, 1)]
//void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
//{
//    // When not using a power of two input we might be out-of-bounds
//    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
//    {
//        return;
//    }
    
//    uint2 index = uint2(dispatchId.x, dispatchId.y);
    
//    float3 normal = MapNormal[index].xyz;    
//    float4 velocity_volume = MapVelocity[index];
//    float3 velocity = velocity_volume.xyz;
//    float volume = velocity_volume.w;
    
//    // How angled the current surface is
//    float localTilt = max(MinLocalTilt, 1.0f - dot(normal, float3(0, 1, 0)));
    
//    // How much sediment we can transport based on a constant factor, the local tilt and velocity of the water
//    // http://www.nlpr.ia.ac.cn/2007papers/gjhy/gh116.pdf page 5
//    float transportCapacity = SedimentCapacity * localTilt * length(velocity);

//    // TODO: do not let this position get lower than any other point around it?
    
//    // If we can transport more than the current volume, we erode. If we can transport less than the current volume we deposit
//    MapHeight[index] += volume - transportCapacity;    
//    //MapHeight[index] = transportCapacity;
//    velocity_volume.w = transportCapacity;
            
    
//    float3 sum = 0.0f;
    
//    [unroll]
//    for (uint i = 0; i < 8; i++)
//    {
//        uint x = i < 4 ? -1 : 1;
//        uint y = i % 2 == 0 ? -1 : 1;
        
//        float4 other = MapVelocity[index + uint2(x, y)];
        
//        float incoming = max(0, dot(other.xyz, normalize(float3(x, 0, y))));
        
//        float3 force = incoming * other.w * other.xyz;
//        sum += force;
//    }
        
//    float3 down = cross(float3(0, -1, 0), normal);
//    sum += down * transportCapacity;
    
//    velocity_volume.xyz = sum / 9.0f;
    
//    if (length(velocity_volume) == 0)
//    {
//        velocity_volume = float4(down.x, down.y, down.z, SedimentCapacity);
//    }
    
//    MapVelocity[index] = velocity_volume;
    
    
//    // Steps
//    // 1.   Given the current velocity (MapVelocity.xyz) and sediment volume (MapVelocity.w)
//    //      increase or decrease the height of the current position. Making sure that the position
//    //      does not get lower than any of the surrounding points (to avoid spikes)
//    // 2.   Update the velocity and sediment volume based on the incoming and outgoing velocities
//    //      I guess we should treat each neighbouring vector that points towards us as a force vector 
//    //      and average them with a vector based on the slope of the current position?
//    // 3.   PROFIT? Run this shader a few thousand times to get results?
    
//}