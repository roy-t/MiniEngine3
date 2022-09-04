#ifndef __WIND
#define __WIND

#include "../../Includes/Defines.hlsl"
#include "SimplexNoise.hlsl"

static const float2x2 m2 = float2x2(0.80, 0.60, -0.60, 0.80);
float FBM(float2 coord, float frequency, float amplitude, float lacunarity, float persistance)
{
    float sum = 0.0f;
    for (int i = 0; i < 3; i++)
    {
        float noise = snoise(coord * frequency) * amplitude;
        frequency *= lacunarity;
        amplitude *= persistance;

        coord = frequency * mul(m2, coord);
        
        sum += noise;
    }
    
    return sum;
}

float GetWindPower(float3 worldPos, float2 facing, float2 windDirection, float windScroll)
{
    float influence = dot(windDirection, facing);    
    influence = max(abs(influence), 0.1f) * sign(influence);
    float noise = FBM(worldPos.xz + float2(windScroll * 4, 0.0f), 0.05f, 1.0f, 1.0f, 0.55f);
    noise = (1.0f + noise) / 2.0f; // from [-1..1] to [0..1]
    return (influence * noise);    
}

#endif