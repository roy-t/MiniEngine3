#ifndef __TAA
#define __TAA

#include "Coordinates.hlsl"
#include "GBuffer.hlsl"

// Inspired by: 
// - https://www.elopezr.com/temporal-aa-and-the-quest-for-the-holy-trail/
// - https://ziyadbarakat.wordpress.com/2020/07/28/temporal-anti-aliasing-step-by-step/
// - https://bartwronski.com/2014/03/15/temporal-supersampling-and-antialiasing/


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
    float3 Resolve(Texture2D depth, Texture2D previous, Texture2D current, sampler textureSampler, float4x4 inverseViewProjection, float4x4 previousViewProjection, float2 uv)
{        
    float3 currentColor = current.Sample(textureSampler, uv).rgb;    
    
    float2 previousUv = Reproject(depth, textureSampler, inverseViewProjection, previousViewProjection, uv);
    float3 previousColor = previous.Sample(textureSampler, previousUv).rgb;
    
    return currentColor * 0.1f + previousColor * 0.9f;    
}

#endif
