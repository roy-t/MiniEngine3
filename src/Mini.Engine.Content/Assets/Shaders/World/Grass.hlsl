#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world :  WORLD;
    float2 texcoord : TEXCOORD;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float4 normal : SV_Target2;
};

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
};
    
StructuredBuffer<float3> Instances : register(t0);
    
#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3)World;
    
    float2 texcoord = float2(uint2(vertexId, vertexId << 1) & 2);
    float4 position = float4(lerp(float2(-1.0f, 1.0f), float2(1.0f, -1.0f), texcoord), 0.0f, 1.0f);

    position += float4(Instances[instanceId], 0);
    
    output.position = mul(WorldViewProjection, position);
    output.world = mul(World, position).xyz;
    output.texcoord = texcoord;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;        
    
    float metalicness = 0.0f;
    float roughness = 1.0f;
    float ambientOcclusion = 0.0f;
        
    output.albedo = float4(0, 1, 0, 1);
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(float3(0, 1, 0)), 1.0f);

    return output;
}