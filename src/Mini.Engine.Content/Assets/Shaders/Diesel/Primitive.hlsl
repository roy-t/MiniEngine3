struct VS_INPUT
{
    float3 position : POSITION;    
    float3 normal : NORMAL;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;    
    // float4 normal : SV_Target1;
};
  
cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;     
    float4x4 World;
    float4 Albedo;
};
    
#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3) World;
    float4 position = float4(input.position, 1.0f);

    output.position = mul(WorldViewProjection, position);
    output.normal = normalize(mul(rotation, input.normal));

    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;
    output.albedo = Albedo;
    
    return output;
}