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
    float MinSpeed;
    float MaxSpeed;
    
    float Gravity;
    float SedimentFactor;
    float2 __Padding;
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
    float speed = MinSpeed;
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
        
        float3 heightAndGradient = ComputeHeightAndGradient(MapHeight, index, Stride);
        float2 gradiant = heightAndGradient.xy;
        float height = heightAndGradient.z;
        
        // On a flat surface the gradiant vector might be {0, 0}
        if (length(gradiant) > 0.0001f)
        {
            gradiant = normalize(gradiant);
        }
                
        // Base the droplets direction on the gradiant and its Inertia
        direction = normalize(direction * Inertia - gradiant * (1.0f - Inertia));
       
        // Scale the direction vector so that the droplet moves to the center of the next pixel
        float deltaX = frac(position.x) + 0.5f;
        if (direction.x > 0.0f)
        {
            deltaX = 1.5f - frac(position.x);
        }
        
        float deltaY = frac(position.y) + 0.5f;
        if (direction.y > 0)
        {
            deltaY = 1.5f - frac(position.y);

        }
        
        float diffX = abs(deltaX / direction.x);
        float diffY = abs(deltaY / direction.y);
            
        direction *= min(diffX, diffY);
                        
        position += direction;

        // Make sure the droplet stays inside the heightmap and border
        uint2 nextIndex = (uint2) position;
        if (nextIndex.x < Border || nextIndex.y < Border || nextIndex.x > (Stride - 1 - Border) || nextIndex.y > (Stride - 1 - Border))
        {
            break;
        }
        
        float deltaHeight = height - MapHeight[nextIndex];        
        float3 normal = ComputeNormalFromHeightMap(MapHeight, nextIndex, Stride);
        float localTilt = 1.0f - dot(normal, float3(0, 1, 0));
        float sedimentCapacity = max(MinSedimentCapacity, speed * sedimentCapacityFactor * localTilt) * water;
        
        if (sedimentCapacity < sediment)
        {            
            float nextHeight = MapHeight[nextIndex];            
            float deposit = sediment - sedimentCapacity;
                        
            int2 offset = -int2(DropletStride / 2, DropletStride / 2);
            
            for (uint i = 0; i < DropletStride * DropletStride; i++)
            {
                int2 depositIndex = ToTwoDimensional(i, DropletStride) + offset + nextIndex;
                MapHeight[depositIndex] += deposit * DropletMask[i];
            }
                        
            sediment -= deposit;
        }
        else if (sedimentCapacity > sediment)
        {
            float erosion = sedimentCapacity - sediment;
            int2 offset = -int2(DropletStride / 2, DropletStride / 2);
            
            for (uint i = 0; i < DropletStride * DropletStride; i++)
            {
                int2 erodeIndex = ToTwoDimensional(i, DropletStride) + offset + nextIndex;
                MapHeight[erodeIndex] -= erosion * DropletMask[i];
            }
            
            sediment += erosion;
        }
                
        float s = sign(deltaHeight);
        float distanceTravelled = length(direction) / Stride;
        float timePassed = distanceTravelled / speed;
        float acceleration = s * localTilt * Gravity * timePassed;
        
        speed = clamp(speed + acceleration, MinSpeed, MaxSpeed);
        water = 1.0f - EasOutQuad(i / (float) iterations);
    }
}
