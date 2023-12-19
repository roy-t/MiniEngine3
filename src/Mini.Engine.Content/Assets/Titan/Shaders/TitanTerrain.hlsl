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
    float4x4 ViewProjection;
    float4x4 World;
};
    
#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    
    float4x4 worldViewProjection = mul(ViewProjection, World);
    output.position = mul(worldViewProjection, float4(input.position, 1.0));
    
    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;
    output.albedo = float4(1.0, 0.0, 0.0, 1.0);

    return output;
}