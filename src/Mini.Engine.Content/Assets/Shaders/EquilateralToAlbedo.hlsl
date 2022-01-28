#include "Includes/Coordinates.hlsl"
#include "Includes/CubeMapStructures.hlsl"

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    float2 uv = WorldToSpherical(normalize(input.world));
    return Texture.SampleLevel(TextureSampler, uv, 0);    
}