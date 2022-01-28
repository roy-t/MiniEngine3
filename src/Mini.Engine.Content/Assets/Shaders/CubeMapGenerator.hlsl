#include "Includes/CubeMapStructures.hlsl"

cbuffer Constants : register(b0)
{
    float4x4 InverseWorldViewProjection;
};

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    output.position = float4(input.position, 1.0f);
    output.world = mul(InverseWorldViewProjection, float4(input.position, 1.0f)).xyz;

    return output;
}