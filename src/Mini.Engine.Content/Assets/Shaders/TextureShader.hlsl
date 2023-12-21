// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);    
Texture2DMS<float4> TextureMS : register(t1);
    
#pragma PixelShader
float4 PS(PS_INPUT input) : SV_Target
{
    return Texture.Sample(TextureSampler, input.tex);
}

#pragma PixelShader
float4 PSMultiSample(PS_INPUT input, float4 position : SV_Position) : SV_Target
{
    // TODO: assume 8 levels for now
    const uint samples = 8;
    float4 accumulator = float4(0, 0, 0, 0);
    for (uint i = 0; i < samples; i++)
    {
        accumulator += TextureMS.Load(position.xy, i);
    }
    
    return accumulator / samples;
}