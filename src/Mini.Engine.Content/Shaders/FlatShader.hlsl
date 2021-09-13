struct VS_INPUT
{
    float3 pos : POSITION;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
};

cbuffer vertexBuffer : register(b0)
{
    float4x4 WorldViewProjection;
};

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = mul(WorldViewProjection, float4(input.pos.xyz, 1.f));
    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return float4(1, 1, 0, 1);
}