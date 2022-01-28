#ifndef __CUBEMAPSTRUCTURES
#define __CUBEMAPSTRUCTURES

struct VS_INPUT
{
    float3 position : POSITION;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : WORLD;
};

#endif
