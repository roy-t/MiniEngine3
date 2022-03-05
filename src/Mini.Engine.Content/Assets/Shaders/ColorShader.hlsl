#include "Includes/Gamma.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;    
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
};

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
    float4 Color;
};

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    
    float4 position = float4(input.position, 1.0f);
    output.position = mul(WorldViewProjection, position);
    return output;

}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return Color;
}