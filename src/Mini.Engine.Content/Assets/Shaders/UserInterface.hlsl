#include "Includes/Gamma.hlsl"

struct VS_INPUT
{
    float2 pos : POSITION;
    float2 tex : TEXCOORD;
    float4 col : COLOR;    
};
            
struct PS_INPUT
{
    float4 pos : SV_POSITION;    
    float2 tex : TEXCOORD;
    float4 col : COLOR;
};

cbuffer Constants : register(b0)
{
    float4x4 ProjectionMatrix;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = mul(ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
    output.col = ToLinear(input.col);
    output.tex = input.tex;
    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    float4 out_col = input.col * Texture.Sample(TextureSampler, input.tex);
    return out_col;
}