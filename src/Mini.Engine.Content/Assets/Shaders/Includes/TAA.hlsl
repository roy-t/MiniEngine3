#ifndef __TAA
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


    float3 BoxClamp(Texture2D current, sampler textureSampler, float3 currentColor, float3 previousColor, float2 uv)
    {
        float width;
        float heigth;
        current.GetDimensions(width, heigth);
        float2 textureSize = float2(width, heigth);
    
    // Arbitrary out of range numbers
        float3 minColor = 9999.0, maxColor = -9999.0;
 
    // Sample a 3x3 neighborhood to create a box in color space
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                float3 color = current.Sample(textureSampler, uv + float2(x, y) / textureSize).xyz; // Sample neighbor
                minColor = min(minColor, color); // Take min and max
                maxColor = max(maxColor, color);
            }
        }
 
    // Clamp previous color to min/max bounding box
        float3 previousColorClamped = clamp(previousColor, minColor, maxColor);
 
    // Blend
        return currentColor * 0.1 + previousColorClamped * 0.9;
    }

    float2 Reproject(Texture2D depth, sampler textureSampler, float4x4 inverseViewProjection, float4x4 previousViewProjection, float2 uv)
    {
        float3 position = ReadPosition(depth, textureSampler, uv, inverseViewProjection);
        if (length(isinf(position)) > 0.0f)
        {
        // TODO: there's a bug when reprojecting the skybox (infinite distance)
        // figure out a way to deal with that
            return uv;
        }
    
        float4 previousProjection = mul(previousViewProjection, float4(position, 1.0f));
        previousProjection /= previousProjection.w;
    
        return ScreenToTexture(previousProjection.xy);
    }
    
    TaaOutput TAA(Texture2D depth, Texture2D previous, Texture2D current, sampler textureSampler, float4x4 inverseViewProjection, float4x4 previousViewProjection, float2 uv)
    {
        float3 currentColor = current.Sample(textureSampler, uv).rgb;
    
        float2 previousUv = Reproject(depth, textureSampler, inverseViewProjection, previousViewProjection, uv);
        float3 previousColor = previous.Sample(textureSampler, previousUv).rgb;
    
        TaaOutput output;
        output.Color = float4(BoxClamp(current, textureSampler, currentColor, previousColor, uv), 1.0f);
        output.Velocity = float2(0, 0);
    
        return output;                
}

#endif
