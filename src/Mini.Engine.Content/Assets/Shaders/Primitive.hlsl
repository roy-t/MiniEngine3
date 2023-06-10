#include "Includes/Gamma.hlsl"
#include "Includes/Normals.hlsl"
#include "Includes/Coordinates.hlsl"
#include "Lighting/Includes/Lights.hlsl"

struct MeshPart
{
    uint Offset;
    uint Length;
    float4 Albedo;
    float Metalicness;
    float Roughness;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float3 normal : NORMAL;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 previousPosition : POSITION0;
    float4 currentPosition : POSITION1;
    float3 world : WORLD;
    float3 normal : NORMAL;
    float4 albedo : COLOR0;
    float metalicness : COLOR1;
    float roughness : COLOR2;
};
    
struct PS_DEPTH_INPUT
{
    float4 position : SV_POSITION;
    float4 world : WORLD;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float4 normal : SV_Target2;
    float2 velocity : SV_Target3;
};
  
cbuffer Constants : register(b0)
{
    float4x4 PreviousViewProjection;
    float4x4 ViewProjection;

    float4x4 PreviousWorld;
    float4x4 World;
        
    float3 CameraPosition;
    
    float2 PreviousJitter;
    float2 Jitter;
    
    uint PartCount;
};
    
StructuredBuffer<float4x4> Instances : register(t0);
StructuredBuffer<MeshPart> Parts : register(t1);
    
#pragma VertexShader
PS_INPUT VSInstanced(VS_INPUT input, uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
    
    // TODO: assumes for TAA that instances don't move around
    float4x4 world = mul(World, Instances[instanceId]);
    float4x4 previousWorld = mul(PreviousWorld, Instances[instanceId]);
    
    float4x4 worldViewProjection = mul(ViewProjection, world);
    float4x4 previousWorldViewProjection = mul(PreviousViewProjection, previousWorld);
    
    float3x3 rotation = (float3x3) world;
    float4 position = float4(input.position, 1.0);

    output.position = mul(worldViewProjection, position);
    output.previousPosition = mul(previousWorldViewProjection, position);
    output.currentPosition = output.position;
    
    output.world = mul(world, position).xyz;
    output.normal = normalize(mul(rotation, input.normal));
    
    for (uint i = 0; i < PartCount; i++)
    {
        if (vertexId >= Parts[i].Offset)
        {
            output.albedo = Parts[i].Albedo;
            output.metalicness = Parts[i].Metalicness;
            output.roughness = Parts[i].Roughness;
        }
    }

    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    float4 albedo = ToLinear(input.albedo.rgba);
    clip(albedo.a - 0.5f);
    
    float3 V = normalize(CameraPosition - input.world);
    float3 normal = normalize(input.normal);
    
    float metalicness = input.metalicness;// 0.0f;
    float roughness = input.roughness; // 0.4f;
    float ambientOcclusion = 1.0f;
    
    input.previousPosition /= input.previousPosition.w;
    input.currentPosition /= input.currentPosition.w;
    float2 previousUv = ScreenToTexture(input.previousPosition.xy - PreviousJitter);
    float2 currentUv = ScreenToTexture(input.currentPosition.xy - Jitter);
        
    OUTPUT output;
    output.albedo = albedo;
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(normal), 1.0f);

    output.velocity = previousUv - currentUv;

    return output;
}
    
cbuffer DepthConstants : register(b1)
{
    float4x4 DepthViewProjection;
    float4x4 DepthWorld;
};

#pragma VertexShader
PS_DEPTH_INPUT VsDepthInstanced(VS_INPUT input, uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_DEPTH_INPUT output;
        
    float4x4 world = mul(DepthWorld, Instances[instanceId]);
    
    float4x4 worldViewProjection = mul(DepthViewProjection, world);
    float4 position = float4(input.position, 1.0);

    output.position = mul(worldViewProjection, position);
    output.world = output.position;
    
    return output;
}

// Unused for cascaded shadow maps, where only the depth stencil is used
#pragma PixelShader
float PsDepth(PS_DEPTH_INPUT input) : SV_TARGET
{
    return input.world.z / input.world.w;
}