// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return Texture.Sample(TextureSampler, input.tex);
}