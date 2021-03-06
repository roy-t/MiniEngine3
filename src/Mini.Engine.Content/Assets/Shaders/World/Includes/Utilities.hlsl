#ifndef __UTILITIES
#define __UTILITIES

float SampleHeight(RWTexture2D<float> heightMap, uint2 position, uint stride)
{
    uint2 index = clamp(position, uint2(0, 0), uint2(stride - 1 , stride - 1));
    return heightMap[index];
}

float3 ComputeHeightAndGradient(RWTexture2D<float> heightMap, float2 position, uint stride)
{
    uint2 index = uint2((uint) position.x, (uint) position.y);
    float2 offset = float2(position.x - index.x, position.y - index.y);
    
    float nw = SampleHeight(heightMap, index + uint2(-1, -1), stride);
    float ne = SampleHeight(heightMap, index + uint2(1, -1), stride);
    float sw = SampleHeight(heightMap, index + uint2(-1, 1), stride);
    float se = SampleHeight(heightMap, index + uint2(1, 1), stride);
        
    float gradientX = (ne - nw) * (1.0f - offset.y) + (se - sw) * offset.y;
    float gradientY = (sw - nw) * (1.0f - offset.x) + (se - ne) * offset.x;

    float c =   nw * (1.0f - offset.x) * (1.0f - offset.y) +
                ne * offset.x * (1.0f - offset.y) +
                sw * (1.0f - offset.x) * offset.y +
                se * offset.x * offset.y;
    
    float length = max(0.01f, sqrt(gradientX * gradientX + gradientY * gradientY));        
    return float3(gradientX / length, gradientY / length, c);
}

float3 ComputeNormalFromHeightMap(RWTexture2D<float> heightMap, uint2 index, uint stride)
{
    float scale = 1.0f / stride;
    
    float2 center = (float2(index.x, index.y) * scale) - float2(0.5f, 0.5f);
    float2 west = center + (float2(-1.0f, 0.0f) * scale);
    float2 north = center + (float2(0.0f, -1.0f) * scale);
    float2 east = center + (float2(1.0f, 0.0f) * scale);
    float2 south = center + (float2(0.0f, 1.0f) * scale);
    
    uint2 uWest = index.xy + uint2(-1, 0);
    uint2 uNorth = index.xy + uint2(0, -1);
    uint2 uEast = index.xy + uint2(1, 0);
    uint2 uSouth = index.xy + uint2(0, 1);
    
    float3 vWest = float3(west.x, SampleHeight(heightMap, uWest, stride), west.y);
    float3 vNorth = float3(north.x, SampleHeight(heightMap, uNorth, stride), north.y);
    float3 vEast = float3(east.x, SampleHeight(heightMap, uEast, stride), east.y);
    float3 vSouth = float3(south.x, SampleHeight(heightMap, uSouth, stride), south.y);
        
    float3 position = (vWest + vNorth + vEast + vSouth) / 4.0f;
    float3 wXn = normalize(cross(position - vNorth, position - vWest));
    float3 eXS = normalize(cross(position - vSouth, position - vEast));
    
    return normalize((wXn + eXS) / 2.0f);
}

#endif