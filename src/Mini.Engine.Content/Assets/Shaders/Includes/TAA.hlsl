#ifndef __TAA
#define __TAA

// Inspired by: 
// - https://www.elopezr.com/temporal-aa-and-the-quest-for-the-holy-trail/
// - https://ziyadbarakat.wordpress.com/2020/07/28/temporal-anti-aliasing-step-by-step/
// - https://bartwronski.com/2014/03/15/temporal-supersampling-and-antialiasing/

float3 Resolve(Texture2D previous, Texture2D current, sampler textureSampler, float2 uv)
{
    float3 previousColor = previous.Sample(textureSampler, uv).rgb;
    float3 currentColor = current.Sample(textureSampler, uv).rgb;    
    float3 color = currentColor * 0.1f + previousColor * 0.9f;
    
    return color;
}

#endif
