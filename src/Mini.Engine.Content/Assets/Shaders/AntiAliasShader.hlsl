#include "includes/FXAA.hlsl"
#include "includes/TAA.hlsl"

// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);    
Texture2D PreviousTexture : register(t1);
Texture2D Depth : register(t2);

cbuffer Constants : register(b0)
{
    float4x4 InverseViewProjection;
    float4x4 PreviousViewProjection;
};

#pragma PixelShader
float4 FxaaPS(PS_INPUT input) : SV_Target
{
    float3 color = FXAA(Texture, TextureSampler, input.tex);
    return float4(color, 1.0f);
}

#pragma PixelShader
float4 NonePS(PS_INPUT input) : SV_Target
{
    float3 color = Texture.Sample(TextureSampler, input.tex).rgb;    
    return float4(color, 1.0f);
}
    
#pragma PixelShader
float4 TaaPS(PS_INPUT input) : SV_Target
{    
    float3 color = Resolve(Depth, PreviousTexture, Texture, TextureSampler, InverseViewProjection, PreviousViewProjection, input.tex);
    return float4(color, 1.0f);
}