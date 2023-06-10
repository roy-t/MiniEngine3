struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;    
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 world : WORLD;
    float2 texcoord : TEXCOORD;
};

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
}

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float4 position = float4(input.position, 1.0f);
    output.position = mul(WorldViewProjection, position);
    output.world = output.position;
    output.texcoord = input.texcoord;

    return output;
}

// Unused for cascaded shadow maps, where only the depth stencil is used
#pragma PixelShader
float PS(PS_INPUT input) : SV_TARGET
{
    float mask = Albedo.Sample(TextureSampler, input.texcoord).w;
    clip(mask - 0.5f);
    
    return input.world.z / input.world.w;
}