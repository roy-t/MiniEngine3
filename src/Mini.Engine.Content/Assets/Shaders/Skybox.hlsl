struct PS_INPUT
{
    float4 position : SV_POSITION;    
    float3 world :  WORLD;    
};

cbuffer Constants : register(b0)
{
    float4x4 InverseWorldViewProjection;
};

sampler TextureSampler : register(s0);
TextureCube CubeMap : register(t0);

// Similar to FullScreenTriangleVs but with an InverseViewProjection
#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VERTEXID)
{
    PS_INPUT output;

    // z is fixed to 1.0f
    float4 pos = float4(vertexId == 1 ? 3.0f : -1.0f, vertexId == 2 ? 3.0f : -1.0f, 0.0f, 1.0f);

    output.position = pos;
    output.world = mul(InverseWorldViewProjection, pos).xyz;

    return output;
}

#pragma PixelShader
float4 PS(PS_INPUT input) : SV_TARGET
{
    return CubeMap.Sample(TextureSampler, input.world);
}