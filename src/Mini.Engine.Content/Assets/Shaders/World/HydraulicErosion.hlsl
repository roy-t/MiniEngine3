#include "../Includes/Indexes.hlsl"
#include "../Includes/Easings.hlsl"
#include "Includes/Utilities.hlsl"

cbuffer Constants : register(b0)
{
    uint Stride;
    uint Border;
    uint PositionsLength;
    uint DropletStride;
    
    float Inertia;
    float MinSedimentCapacity;
    
    float Gravity;
    float SedimentFactor;
    float DepositSpeed;
    float3 __Padding;
}

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapTint : register(u1);
StructuredBuffer<float2> Positions : register(t0);
StructuredBuffer<float> DropletMask : register(t1);

#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    if (dispatchId.x >= PositionsLength)
    {
        return;
    }

    // These variables are scaled so that the erosion looks roughly the same strength, 
    // independent of the size of the heightmap.
    uint iterations = Stride / 12u;
    float sedimentCapacityFactor = (Stride / 51200.0f) * SedimentFactor;
    
    // Droplet property initialization
    float speed = 0.01f;
    float water = 1.0f;      
    float evaporation = water / iterations;
    float sediment = 0.0f;
    
    // Get a randomized position from the input buffer and place it at the center of the pixel
    float2 position = Positions[dispatchId.x];
    
    // TODO: initial direction has a chance to push a droplet 'up' a cliff for 1 step
    // Make sure that droplets move, through Inertia, even when 
    // they start on a flat surface
    float2 direction = normalize(position - (Stride * 0.5f));
    
    uint2 startIndex = (uint2) position;
    for (uint i = 0; i < iterations; i++)
    {
        uint2 index = (uint2) position;
        float2 cellOffset = position - index;
        
        float3 heightAndGradient = ComputeHeightAndGradient(MapHeight, position, Stride);
        float2 gradiant = heightAndGradient.xy;
        float height = heightAndGradient.z;
        
        // Base the droplets direction on the gradiant and its Inertia
        direction = direction * Inertia - gradiant * (1.0f - Inertia);
                
        if (length(direction) <= 0.0001f || index.x < Border || index.y < Border || index.x > (Stride - 1 - Border) || index.y > (Stride - 1 - Border))
        {
            break;
        }
        
        direction = normalize(direction);        
        position += direction;
        
        float newHeight = ComputeHeightAndGradient(MapHeight, position, Stride).z;
        float deltaHeight = height - newHeight;
        float3 normal = ComputeNormalFromHeightMap(MapHeight, index, Stride);
        float localTilt = 1.0f - dot(normal, float3(0, 1, 0));
        float sedimentCapacity = max(MinSedimentCapacity, speed * sedimentCapacityFactor * localTilt * water);
        
        if (sedimentCapacity < sediment || deltaHeight < 0)
        {            
            float nextHeight = MapHeight[index];
            float deposit = (deltaHeight < 0)
                ? min(abs(deltaHeight), sediment)
                : sediment - sedimentCapacity;
            
            deposit *= DepositSpeed;
            int2 offset = -int2(DropletStride / 2, DropletStride / 2);
            
            MapHeight[index] += deposit * (1.0f - cellOffset.x) * (1.0f - cellOffset.y);
            MapHeight[index + uint2(1, 0)] += deposit * cellOffset.x * (1.0f - cellOffset.y);
            MapHeight[index + uint2(0, 1)] += deposit * (1.0f - cellOffset.x) * cellOffset.y;
            MapHeight[index + uint2(1, 1)] += deposit * cellOffset.x * cellOffset.y;
                        
            sediment -= deposit;
        }
        else if (sedimentCapacity > sediment)
        {
            float erosion = min(sedimentCapacity - sediment, abs(deltaHeight));
            int2 offset = -int2(DropletStride / 2, DropletStride / 2);
            
            for (uint i = 0; i < DropletStride * DropletStride; i++)
            {
                int2 erodeIndex = ToTwoDimensional(i, DropletStride) + offset + index;
                MapHeight[erodeIndex] -= erosion * DropletMask[i];
            }
            
            sediment += erosion;
        }
        
        speed = sqrt(max(0, speed * speed + deltaHeight * Gravity));
        water = 1.0f - EasOutQuad(i / (float) iterations);
    }
}
