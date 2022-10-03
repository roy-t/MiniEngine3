// Use with FullScreenTriangle.TextureVS

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

sampler TextureSampler : register(s0);
Texture2D Texture : register(t0);

// Different tone mapping functions based on https://64.github.io/tonemapping/
float3 reinhard(float3 v)
{
    return v / (float3(1.0f, 1.0f, 1.0f) + v);
}

float3 reinhard_jodie(float3 v)
{
    float l = dot(v, float3(0.2126f, 0.7152f, 0.0722f));
    float3 tv = v / (1.0f + v);
    return lerp(v / (1.0f + l), tv, tv);
}

float3 aces_approx(float3 v)
{
    v *= 0.6f;
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return clamp((v * (a * v + b)) / (v * (c * v + d) + e), 0.0f, 1.0f);
}

float3 uncharted2_tonemap_partial(float3 x)
{
    float A = 0.15f;
    float B = 0.50f;
    float C = 0.10f;
    float D = 0.20f;
    float E = 0.02f;
    float F = 0.30f;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 uncharted2_filmic(float3 v)
{
    float exposure_bias = 2.0f;
    float3 curr = uncharted2_tonemap_partial(v * exposure_bias);

    float3 W = float3(11.2f, 11.2f, 11.2f);
    float3 white_scale = float3(1.0f, 1.0f, 1.0f) / uncharted2_tonemap_partial(W);
    return curr * white_scale;
}

#pragma PixelShader
float4 ToneMap(PS_INPUT input) : SV_Target
{        
    float3 color = Texture.Sample(TextureSampler, input.tex).rgb;
    //color = reinhard(color); // Guarantees [0..1] but makes everything a bit gray/dull
    //color = reinhard_jodie(color); // better with colours
    //color = uncharted2_filmic(color); // filmic, tweakable, but slightly gray in current settings
    color = aces_approx(color); // high contrast, makes things pop, pretty dark
    
    
    return float4(color, 1.0f);
}