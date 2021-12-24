struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

cbuffer vertexBuffer : register(b0)
{
    float4x4 WorldViewProjection;
};

sampler textureSampler : register(s0);
Texture2D albedo : register(t0);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.position = mul(WorldViewProjection, float4(input.position.xyz, 1.f));
    output.texcoord = input.texcoord;
    output.normal = input.normal;
    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    float4 color = albedo.Sample(textureSampler, input.texcoord);
    return color;
}