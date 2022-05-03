#include "../Includes/Indexes.hlsl"
#include "../Includes/Easings.hlsl"
#include "Includes/Utilities.hlsl"

cbuffer DropletConstants : register(b0)
{
    uint Dimensions;
    uint Border;
    uint PositionsLength;
    uint BrushStride;
}

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapTint : register(u1);
StructuredBuffer<float2> Positions : register(t0);
StructuredBuffer<float> Brush : register(t1);

#pragma ComputeShader
[numthreads(8, 8, 1)]
void Kernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    if (dispatchId.x >= PositionsLength)
    {
        return;
    }
    
    // TODO: make these CBuffer variables    
    
    // Inertia. 
    // Controls how much water keeps going the same direction. Lower numbers make the water follow the contours of the 
    // terrain better. Higher numbers allow the water to maintain its momentum and even allow it to flow slightly up
    // Range: [0..1]
    const float Inertia = 0.55f; 
    
    // MinSedimentCapacity 
    //Sediment capacity of slow moving or standing still water. Lower numbers prevent cratering but might stop a droplet
    // from affecting the terrain before the end of its lifetime. Higher numbers sometimes lead to craters and hills forming
    // on flat surfaces.
    // Range: [0..0.01]
    const float MinSedimentCapacity = 0.001f;
    
    // MinSpeed
    // Minimum speed, in meters per second, that water flows at. The speed of the water affects its sediment capacity.
    // Lower numbers create more deposits and thus a rougher terrain. Higher numbers create a smoother terrain with more erosion.
    // Range: [0.0025f..1.0f]
    const float MinSpeed = 0.01f;
    
    // MaxSpeed
    // Maximum speed, in meters per second, that water flows at. The speed of the water affects its sediment capacity.
    // Lower numbers create more deposits and thus a rougher terrain. Higher numbers create a smoother terrain with more erosion.
    // Range: [1.0f..10.0f]
    const float MaxSpeed = 7.0f;
    
    // Gravity
    // Affects the acceleration over time of water that is going up or down hill. Lower numbers reduce the effect on steep terrain.
    // Higher numbers increase the effect on steep terrain.
    // Range [1.0f, 20.0f]
    const float Gravity = 4.0f;
    
    // SedimentFactor
    // Multiplier for the amount of sediment one droplet of water can carry. Lower numbers produce a softer effect. Higher
    // numbers produce a stronger effect.
    // Range: [0.01f..5.0f]
    const float SedimentFactor = 1.0f;
    
    
    // These variables are scaled so that the erosion looks roughly the same strength, 
    // independent of the size of the heightmap.
    uint MaxLifeTime = Dimensions / 12u;
    float sedimentCapacityFactor = (Dimensions / 51200.0f) * SedimentFactor;
    
    // Droplet property initialization
    float speed = MinSpeed;
    float water = 1.0f;      
    float evaporation = water / MaxLifeTime;
    float sediment = 0.0f;
    
    // Get a randomized position from the input buffer and place it at the center of the pixel
    float2 position = Positions[dispatchId.x];
    
    // TODO: initial direction has a chance to push a droplet 'up' a cliff for 1 step
    // Make sure that droplets move, through Inertia, even when 
    // they start on a flat surface
    float2 direction = normalize(position - (Dimensions * 0.5f));
    
    uint2 startIndex = (uint2) position;
    for (uint i = 0; i < MaxLifeTime; i++)
    {
        uint2 index = (uint2) position;
        
        float3 heightAndGradient = ComputeHeightAndGradient(MapHeight, index, Dimensions);
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
        if (nextIndex.x < Border || nextIndex.y < Border || nextIndex.x > (Dimensions - 1 - Border) || nextIndex.y > (Dimensions - 1 - Border))
        {
            break;
        }
        
        float deltaHeight = height - MapHeight[nextIndex];        
        float3 normal = ComputeNormalFromHeightMap(MapHeight, nextIndex, Dimensions);
        float localTilt = 1.0f - dot(normal, float3(0, 1, 0));
        float sedimentCapacity = max(MinSedimentCapacity, speed * sedimentCapacityFactor * localTilt) * water;
        
        if (sedimentCapacity < sediment)
        {            
            float nextHeight = MapHeight[nextIndex];            
            float deposit = sediment - sedimentCapacity;
                        
            int2 offset = -int2(BrushStride / 2, BrushStride / 2);
            
            for (uint i = 0; i < BrushStride * BrushStride; i++)
            {
                int2 depositIndex = ToTwoDimensional(i, BrushStride) + offset + nextIndex;
                MapHeight[depositIndex] += deposit * Brush[i];
            }
                        
            sediment -= deposit;
        }
        else if (sedimentCapacity > sediment)
        {
            float erosion = sedimentCapacity - sediment;
            int2 offset = -int2(BrushStride / 2, BrushStride / 2);
            
            for (uint i = 0; i < BrushStride * BrushStride; i++)
            {
                int2 erodeIndex = ToTwoDimensional(i, BrushStride) + offset + nextIndex;
                MapHeight[erodeIndex] -= erosion * Brush[i];
            }
            
            sediment += erosion;
        }
                
        float s = sign(deltaHeight);
        float distanceTravelled = length(direction) / Dimensions;
        float timePassed = distanceTravelled / speed;
        float acceleration = s * localTilt * Gravity * timePassed;
        
        speed = clamp(speed + acceleration, MinSpeed, MaxSpeed);
        water = 1.0f - EasOutQuad(i / (float) MaxLifeTime);
    }
}
