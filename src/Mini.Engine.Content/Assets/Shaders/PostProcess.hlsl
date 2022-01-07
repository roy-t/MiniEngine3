struct VS_INPUT
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = float4(input.pos.xyz, 1.0f);
    output.tex = input.tex;

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return Texture.Sample(TextureSampler, input.tex);
}