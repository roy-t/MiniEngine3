#include "Include.hlsl"

cbuffer vertexBuffer : register(b0)
{
    float4x4 ProjectionMatrix;
    float Bar[110,10,10];
    VS_INPUT Foo;
};

sampler sampler0;
Texture2D texture0;
            
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = mul(ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
    output.col = input.col;
    output.uv = input.uv;
    return output;
}
            
float4 PS(PS_INPUT input) : SV_Target
{
    float4 out_col = input.col * texture0.Sample(sampler0, input.uv);
    return out_col;
}