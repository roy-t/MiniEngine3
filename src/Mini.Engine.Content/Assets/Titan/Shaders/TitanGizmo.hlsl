struct VS_INPUT
{
    float3 position : POSITION;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
};
    
cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
}
    
#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    output.position = mul(WorldViewProjection, float4(input.position, 1.0));
    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input, uint primitiveId : SV_PrimitiveID)
{
    OUTPUT output;
    output.albedo = float4(1.0f, 0.0f, 0.0f, 1.0f);

    return output;
}