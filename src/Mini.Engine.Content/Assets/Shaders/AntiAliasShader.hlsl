#include "includes/FXAA.hlsl"
#include "includes/TAA.hlsl"

// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Color : register(t0);    
Texture2D PreviousColor: register(t1);
Texture2D Velocity : register(t2);
Texture2D PreviousVelocity : register(t3);
    
    
Texture2D Depth : register(t4);

cbuffer Constants : register(b0)
{
    float4x4 InverseViewProjection;
    float4x4 PreviousViewProjection;
};

#pragma PixelShader
float4 FxaaPS(PS_INPUT input) : SV_Target
{
    float3 color = FXAA(Color, TextureSampler, input.tex);
    return float4(color, 1.0f);
}

#pragma PixelShader
float4 NonePS(PS_INPUT input) : SV_Target
{
    float3 color = Color.Sample(TextureSampler, input.tex).rgb;
    return float4(color, 1.0f);
}
    
#pragma PixelShader
TaaOutput TaaPS(PS_INPUT input)
{    
    TaaOutput output = TAA(Depth, PreviousColor, Color, TextureSampler, InverseViewProjection, PreviousViewProjection, input.tex);
    return output;
}