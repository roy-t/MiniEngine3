// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);    
Texture2DMS<float4> TextureMS : register(t1);
 
cbuffer Constants : register(b0)
{
    uint Samples;
};
    
#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return Texture.Sample(TextureSampler, input.tex);
}

#pragma PixelShader
float4 PSMultiSample(PS_INPUT input, float4 position : SV_Position) : SV_Target
{    
    float4 accumulator = float4(0, 0, 0, 0);
    for (uint i = 0; i < Samples; i++)
    {
        accumulator += TextureMS.Load(position.xy, i);
    }
    
    return accumulator / Samples;
}