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
    float __Padding2;
}

RWTexture2D<float> MapHeight : register(u0);
RWTexture2D<float4> MapTint : register(u1);
StructuredBuffer<float2> Positions : register(t0);

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

#pragma ComputeShader
[numthreads(8, 8, 1)]
void Droplet(in uint3 dispatchId : SV_DispatchThreadID)
{
    if (dispatchId.x >= PositionsLength)
    {
        return;
    }
    
    // TODO: make these CBuffer variables
    const uint MaxLifeTime = 300;
    const float inertia = 0.55f;
        
    // Get a randomized position from the input buffer and place it at the center of the pixel
    float2 position = Positions[dispatchId.x];
    
    // Make sure that droplets move, through inertia, even when 
    // they start on a flat surface
    float2 direction = normalize(position - PositionsLength);
    
    uint2 startIndex = (uint2) position;
    MapTint[startIndex] = float4(1, 1, 1, 1);
    
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
        
        MapTint[(uint2) position] = float4(1, 0, 0, 1);
    }
                
    uint2 endIndex = (uint2) position;
    MapTint[endIndex] = float4(0, 0, 0, 1);    
}

//RWTexture2D<float4> MapVelocityIn : register(u1);
//RWTexture2D<float4> MapVelocityOut : register(u2);
//RWTexture2D<float4> MapMassIn : register(u3);
//RWTexture2D<float4> MapMassOut : register(u4);

//static const float SedimentCapacity = 0.001f;
//static const float MinLocalTilt = 0.1f;

//static const float Gravity = 9.81;
//static const float3 Up = float3(0.0f, 1.0f, 0.0f);
//static const float3 Down = float3(0.0f, -1.0f, 0.0f);


//float3 ComputeHeightAndGradient(uint2 index, float2 position)
//{    
//    float x = position.x - float(index.x);
//    float y = position.y - float(index.y);
                  
//    float nw = MapHeight[index + uint2(-1, -1)];
//    float ne = MapHeight[index + uint2(1, -1)];
//    float se = MapHeight[index + uint2(1, 1)];
//    float sw = MapHeight[index + uint2(-1, 1)];
    
//    float gradientX = (ne - nw) * (1.0f - y) + (se - sw) * y;
//    float gradientY = (sw - nw) * (1.0f - x) + (se - ne) * x;

//    float c = nw * (1.0f - x) * (1.0f - y) + ne * x * (1.0f - y) + sw * (1.0f - x) * y + se * x * y;
    
//    return float3(gradientX, gradientY, c);    
//}

//#pragma ComputeShader
//[numthreads(8, 8, 1)]
//void Droplet(in uint3 dispatchId : SV_DispatchThreadID)
//{
//    uint border = 2;
//    float2 direction = float2(0, 0);    
//    float speed = 0.75f;
//    float water = 0.1f;
//    float sediment = 0.0f;
//    float inertia = 0.25f;
//    float sedimentCapacityFactor = 0.1f;
//    float minSedimentCapacity = 0.01f;
//    float depositSpeed = 0.2f;
//    float erodeSpeed = 0.2f;
//    uint maxLifeTime = 30;
//    float evaporationSpeed = 1.0f / maxLifeTime;
    
//    // TODO: I think the code sort of works now, but the variables are completely wrong probaby
//    // see also: https://github.com/SebLague/Hydraulic-Erosion/blob/master/Assets/Scripts/ComputeShaders/Erosion.compute
//    float2 position = float2(X, Y);
//    for (uint lifeTime = 0; lifeTime < maxLifeTime; lifeTime++)
//    {
//        uint2 index = uint2(position.x, position.y);
//        float2 offset = float2(position.x - index.x, position.y - index.y);
        
//        // Calculate the droplet's height and the direction of flow
//        float3 heightAndGradiant = ComputeHeightAndGradient(index, position);
//        float2 gradiant = heightAndGradiant.xy;
//        float height = heightAndGradiant.z;
//        // Update the droplet's position
//        direction.x = (direction.x * inertia - gradiant.x * (1.0f - inertia));
//        direction.y = (direction.y * inertia - gradiant.y * (1.0f - inertia));
                
//        if (length(gradiant) < 0.00001f)
//        {
//            MapHeight[index] = 10;
//            break;
//        }                
        
//        direction = normalize(direction);
        
//        position += direction;
//        uint2 nextIndex = uint2(position.x, position.y);
//        if (nextIndex.x < border || nextIndex.y < border || nextIndex.x > (Dimensions - 1 - border) || nextIndex.y > (Dimensions - 1 - border))
//        {
//            MapHeight[index] = -10;
//            break;
//        }
        
//        float newHeight = MapHeight[index];
//        float delta = newHeight - height;
        
//        float sedimentCapacity = max(-delta * speed * water * sedimentCapacityFactor, minSedimentCapacity);
        
//        if (sediment > sedimentCapacity || delta > 0)
//        {
//            float deposit = (delta > 0) ? min(delta, sediment) : (sediment - SedimentCapacity) * depositSpeed;
//            sediment -= deposit;
            
//            MapHeight[index + uint2(-1, -1)] += deposit * (1.0f - offset.x) * (1.0f - offset.y);
//            MapHeight[index + uint2(1, -1)] += deposit * offset.x * (1.0f - offset.y);
//            MapHeight[index + uint2(1, 1)] += deposit * (1.0f - offset.x) * offset.y;
//            MapHeight[index + uint2(-1, 1)] += deposit * offset.x * offset.y;
//        }
//        else
//        {
//            float erosion = min((sedimentCapacity - sediment) * erodeSpeed, -delta);
//            MapHeight[index + uint2(-1, -1)] = erosion * 0.25f;
//            MapHeight[index + uint2(1, -1)] -= erosion * 0.25f;
//            MapHeight[index + uint2(1, 1)] -= erosion * 0.25f;
//            MapHeight[index + uint2(-1, 1)] -= erosion * 0.25f;
            
//            sediment += erosion;
//        }
        
//        speed = sqrt(max(0, speed * speed + delta * 9.81f));
//        water *= (1.0f - evaporationSpeed);
        
//        // Find the droplet's new height and calculate the delta
        
//        // Calculate the droplet's sediment capacity
        
//        // - If carrying more sediment than capacity, or if flowing up a slope
//        // deposit a fraction of the sediment to the surrounding nodes (with bilinear interpolation)
        
//        // - Otherwise
//        // erode a fraction of the droplet's remaining capacity from the soild, distributed over the radius of the droplet
//        // Note: don't erode more than deltaHeight to avoid digging holes behind the droplet and creating spikes
        
//        // Update droplet's speed base don deltaHeight
//        // Evaporate a fraction of the droplet's water
//    }
//}

//float ComputeLocalTilt(float3 normal)
//{
//    return max(MinLocalTilt, 1.0f - dot(normal, Up));
//}

//float3 ComputeFlowDirection(float3 normal)
//{
//    return cross(Down, normal);
//}

//float ComputeTransportCapacity(float3 normal)
//{
//    const float force = 1.0f;
//    float localTilt = ComputeLocalTilt(normal);
    
//    return SedimentCapacity * localTilt * force;
//}

//// Start with 1 unit of water on every pixel
//#pragma ComputeShader
//[numthreads(8, 8, 1)]
//void Seed(in uint3 dispatchId : SV_DispatchThreadID)
//{
//    // When not using a power of two input we might be out-of-bounds
//    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
//    {
//        return;
//    }
        
//    uint2 index = uint2(dispatchId.x, dispatchId.y);
    
//    float3 normal = ComputeNormalFromHeightMap(MapHeight, dispatchId.xy, Stride);
//    float3 flowDirection = ComputeFlowDirection(normal);
//    float water = SedimentCapacity * MinLocalTilt;
//    float mass = ComputeTransportCapacity(normal);
//    MapVelocityOut[index] = float4(flowDirection, 0.0f);
//    MapMassOut[index] = float4(water, mass, 0.0f, 0.0f);
//}


//#pragma ComputeShader
//[numthreads(8, 8, 1)]
//void Erode(in uint3 dispatchId : SV_DispatchThreadID)
//{
//    // When not using a power of two input we might be out-of-bounds
//    if (dispatchId.x >= Stride || dispatchId.y >= Stride)
//    {
//        return;
//    }
    
//    uint2 index = uint2(dispatchId.x, dispatchId.y);            
    
//    // Check how much water and mass has flowed to our tile
//    float water = MapMassIn[index].x;
//    float absorbedSediment = MapMassIn[index].y;
                        
//    // Check how much mass we can transport
//    float3 normal = ComputeNormalFromHeightMap(MapHeight, index, Stride);
//    float transportCapacity = ComputeTransportCapacity(normal) * water;
        
//    // Erode or deposit based on how much mass the water transports away from here
//    if (transportCapacity < absorbedSediment)
//    {
//        float deposit = absorbedSediment - transportCapacity;
//        MapHeight[index] += deposit;
//    }
    
//    if (transportCapacity > absorbedSediment)
//    {
//        // TODO: don't erode further than neighbours
//        float erosion = transportCapacity - absorbedSediment;
//        MapHeight[index] -= erosion;
//    }
    
//    // Now that all water, and the mass in it has flown away, update with how much we get from our neighbours
//    water = 0;
//    absorbedSediment = 0;
    
//    [unroll]
//    for (uint i = 0; i < 8; i++)
//    {
//        uint x = i < 4 ? -1 : 1;
//        uint y = i % 2 == 0 ? -1 : 1;
        
//        uint2 position = index + uint2(x, y);
        
//        // Compute how much of the water/abasored mass on the neighbouring tile is going to flow our way
//        float3 velocityIn = MapVelocityIn[position].xyz;
//        float factor = max(0, dot(normalize(float3(velocityIn.x, 0, velocityIn.z)), normalize(-float3(x, 0, y))));
        
//        // Update our water and mass values
//        water += factor * MapMassIn[position].x;
//        absorbedSediment += factor * MapMassIn[position].y;
//    }       
    
//    float3 flowDirection = ComputeFlowDirection(normal);
    
//    MapVelocityOut[index] = float4(flowDirection, 0);
//    MapMassOut[index] = float4(water, absorbedSediment, 0, 0);
//}

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