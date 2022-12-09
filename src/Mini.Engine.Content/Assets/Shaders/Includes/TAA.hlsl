﻿#ifndef __TAA
#define __TAA

#include "GBuffer.hlsl"

struct TaaOutput
{
    float4 Color : SV_Target0;
    float2 Velocity : SV_Target1;
};

// Inspired by: 
// - https://www.elopezr.com/temporal-aa-and-the-quest-for-the-holy-trail/
// - https://ziyadbarakat.wordpress.com/2020/07/28/temporal-anti-aliasing-step-by-step/
// - https://bartwronski.com/2014/03/15/temporal-supersampling-and-antialiasing/

// Store weight in w component
float4 AdjustHDRColor(float3 color)
{
    float luminance = dot(color, float3(0.299, 0.587, 0.114));
    float luminanceWeight = 1.0 / (1.0 + luminance);
    return float4(color, 1.0) * luminanceWeight;
}

float3 BoxClamp(Texture2D current, sampler textureSampler, float3 currentColor, float3 previousColor,  float velocityDisocclusion, float2 uv)
{
    float width;
    float heigth;
    current.GetDimensions(width, heigth);
    float2 textureSize = float2(width, heigth);
        
    float3 minColor = float3(9999.0f, 9999.0f, 9999.0f);
    float3 maxColor = float3(-9999.0f, -9999.0f, -9999.0f);
 
    
    float3 blur = float3(0.0f, 0.0f, 0.0f);
    // Sample a 3x3 neighborhood to create a box in color space
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            //float3 color = current.Sample(textureSampler, uv + float2(x, y) / textureSize).xyz; // Sample neighbor
            float3 samp = current.Sample(textureSampler, uv + float2(x, y) / textureSize).xyz;
            float3 color = samp;// or AdjustHDRColor(samp).xyz;? 'read neighborhood is not very clear'
            minColor = min(minColor, color); // Take min and max
            maxColor = max(maxColor, color);
            
            blur += color; // or samp?
        }
    }
    
    blur /= 9.0f;
 
    // Clamp previous color to min/max bounding box
    float3 previousColorClamped = clamp(previousColor, minColor, maxColor);

    float4 pc = AdjustHDRColor(previousColor);
    float4 cc = AdjustHDRColor(currentColor);

    float previousWeight = 0.9 * pc.a;
    float currentWeight = 0.1 * cc.a;
     
    // Blend
    float3 accumulation = currentColor * currentWeight + previousColorClamped * previousWeight;
    accumulation /= (previousWeight + currentWeight);
    
    return lerp(accumulation, blur, velocityDisocclusion);
}

float2 GetCameraVelocity(Texture2D depth, sampler textureSampler, float4x4 inverseViewProjection, float4x4 previousViewProjection, float2 uv)
{
    float3 position = ReadPosition(depth, textureSampler, uv, inverseViewProjection);

    // objects at infinite distance don't care about camera movement
    if (length(isinf(position)) > 0)
    {
        return float2(0.0f, 0.0f);
    }

    float4 previousProjection = mul(previousViewProjection, float4(position, 1.0f));
    previousProjection /= previousProjection.w;
    
    float2 prevUv = ScreenToTexture(previousProjection.xy);
    return (prevUv - uv);
}

float GetVelocityDisocclusion(float2 previousVelocity, float2 currentVelocity)
{
    float velocityLength = length(previousVelocity - currentVelocity);
    return saturate((velocityLength - 0.001f) * 10.0f);
}
    
TaaOutput TAA(Texture2D depth, Texture2D colorHistory, Texture2D colorCurrent, Texture2D velocityHistory, Texture2D velocityCurrent, sampler textureSampler, float4x4 inverseViewProjection, float4x4 previousViewProjection, float2 uv)
{
    float3 currentColor = colorCurrent.Sample(textureSampler, uv).rgb;
        
    float2 cameraVelocity = GetCameraVelocity(depth, textureSampler, inverseViewProjection, previousViewProjection, uv);     
    float2 colorVelocity = velocityCurrent.Sample(textureSampler, uv).xy;
    float2 velocity = cameraVelocity + colorVelocity;
    float2 previousUv = uv + velocity;
    
    float2 previousVelocity = velocityHistory.Sample(textureSampler, previousUv).xy;
    float3 previousColor = colorHistory.Sample(textureSampler, previousUv).rgb;
    
    float velocityDisocclusion = GetVelocityDisocclusion(previousVelocity, velocity);
    
    TaaOutput output;
    output.Color = float4(BoxClamp(colorCurrent, textureSampler, currentColor, previousColor, velocityDisocclusion, uv), 1.0f);
    output.Velocity = float2(0, 0);
    
    return output;
}

#endif