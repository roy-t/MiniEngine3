#include "../Includes/Indexes.hlsl"
#include "Includes/Utilities.hlsl"

cbuffer ErosionConstants : register(b0)
{    
    uint Stride;
    float3 __Padding;
};

cbuffer DropletConstants : register(b1)
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

float3 ComputeHeightAndGradient(uint2 index)
{                  
    float nw = MapHeight[index + uint2(-1, -1)];
    float ne = MapHeight[index + uint2(1, -1)];
    float se = MapHeight[index + uint2(1, 1)];
    float sw = MapHeight[index + uint2(-1, 1)];
    
    float gradientX = (ne - nw) * 0.5f + (se - sw) * 0.5f;
    float gradientY = (sw - nw) * 0.5f + (se - ne) * 0.5f;

    float c = MapHeight[index];
    
    return float3(gradientX, gradientY, c);
}

float EasOutQuad(float x)
{
    return 1 - (1 - x) * (1 - x);
}

#pragma ComputeShader
[numthreads(8, 8, 1)]
void Droplet(in uint3 dispatchId : SV_DispatchThreadID)
{
    if (dispatchId.x >= PositionsLength)
    {
        return;
    }
    
    // TODO: make these CBuffer variables
    // TODO: for different map sizes this needs a lot of tweaking, try to make variables map size independent
    // MaxLifetime is probably one of the more variable variants
    const uint MaxLifeTime = 75;
    const float inertia = 0.55f;
    const float sedimentCapacityFactor = 0.02f;
    const float minSedimentCapacity = 0.001f;
    const float MinSpeed = 0.01f;
    const float MaxSpeed = 7.0f;
    const float depositSpeed = 0.05f;
    
    float speed = MinSpeed;
    float water = 1.0f;      
    float evaporation = water / MaxLifeTime;
    float sediment = 0.0f;
    
    // Get a randomized position from the input buffer and place it at the center of the pixel
    float2 position = Positions[dispatchId.x];
    
    // TODO: initial direction has a chance to push a droplet 'up' a cliff for 1 step
    // Make sure that droplets move, through inertia, even when 
    // they start on a flat surface
    float2 direction = normalize(position - PositionsLength);
    
    uint2 startIndex = (uint2) position;
    for (uint i = 0; i < MaxLifeTime; i++)
    {
        uint2 index = (uint2) position;
        
        float3 heightAndGradient = ComputeHeightAndGradient(index);
        float2 gradiant = heightAndGradient.xy;
        float height = heightAndGradient.z;
        
        // On a flat surface the gradiant vector might be {0, 0}
        if (length(gradiant) > 0.0001f)
        {
            gradiant = normalize(gradiant);
        }
                
        // Base the droplets direction on the gradiant and its inertia
        direction = normalize(direction * inertia - gradiant * (1.0f - inertia));
       
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
        
        //float deltaHeight =   MapHeight[nextIndex] - height;
        float3 normal = ComputeNormalFromHeightMap(MapHeight, nextIndex, Dimensions);
        float localTilt = 1.0f - dot(normal, float3(0, 1, 0));
        float sedimentCapacity = max(minSedimentCapacity, speed * sedimentCapacityFactor * localTilt) * water;
        
        if (sedimentCapacity < sediment && i > 3)
        {            
            float nextHeight = MapHeight[nextIndex];
            float diff = nextHeight - height;
            float deposit = sediment - sedimentCapacity;
            if (diff > 0)
            {
                deposit = min(deposit * depositSpeed, diff);
            }
            else
            {
                deposit = deposit * depositSpeed;
            }
            
            sediment -= deposit;
            MapHeight[nextIndex] += deposit;
           
        }
        else if (sedimentCapacity > sediment && i > 3) // TODO: hack, i > 3 because apparently start makes deep holes?
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
                           
        float deltaHeight = height - MapHeight[nextIndex];
        float gravity = 4.0f;
        float s = sign(deltaHeight);
        float distanceTravelled = length(direction) / Dimensions;
        float timePassed = distanceTravelled / speed;
        float acceleration = s * localTilt * gravity * timePassed;
        
        speed = max(MinSpeed, min(MaxSpeed, speed + acceleration));                
        water = 1.0f - EasOutQuad(i / (float) MaxLifeTime);
    }
                
    uint2 endIndex = (uint2) position;            
}
