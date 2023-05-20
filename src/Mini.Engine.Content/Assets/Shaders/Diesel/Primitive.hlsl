#include "../Includes/Gamma.hlsl"
#include "../Lighting/Includes/Lights.hlsl"

struct MeshPart
{
    uint Offset;
    uint Length;
    float4 Color;
};
    
struct VS_INPUT
{
    float3 position : POSITION;
    float3 normal : NORMAL;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : WORLD;
    float3 normal : NORMAL;
    float4 Albedo : COLOR0;
};

struct OUTPUT
{
    float4 color : SV_Target0;
};
  
cbuffer Constants : register(b0)
{
    float4x4 World;
    float4x4 ViewProjection;
    float3 CameraPosition;
    uint PartCount;
};
    
StructuredBuffer<float4x4> Instances : register(t0);
StructuredBuffer<MeshPart> Parts : register(t1);
    
#pragma VertexShader
PS_INPUT VSInstanced(VS_INPUT input, uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
    
    float4x4 world = mul(World, Instances[instanceId]);
    float4x4 worldViewProjection = mul(ViewProjection, world);
    
    float3x3 rotation = (float3x3) world;
    float4 position = float4(input.position, 1.0);

    output.position = mul(worldViewProjection, position);
    output.world = mul(world, position).xyz;
    output.normal = normalize(mul(rotation, input.normal));
    
    for (uint i = 0; i < PartCount; i++)
    {
        if (vertexId >= Parts[i].Offset)
        {
            output.Albedo = Parts[i].Color;
        }
    }

    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;
        
    const float3 lights[] =
    {
        normalize(float3(0.5, -0.8, 0.0)),
        normalize(float3(-0.5, -0.8, 0.0)),
        normalize(float3(0.0, -0.8, 0.5)),
        normalize(float3(0.0, -0.8, -0.5)),
        normalize(float3(0.0, 1.0, 0.0)),
    };
    
    const float3 colors[] =
    {
        ToLinear(float3(1.0, 0.9, 0.9)),
        ToLinear(float3(1.0, 0.9, 0.9)),
        ToLinear(float3(0.9, 0.9, 1.0)),
        ToLinear(float3(0.9, 0.9, 1.0)),
        ToLinear(float3(0.7, 0.7, 0.7)),
    };
    
    const float Strength = 1.0f;
    float3 albedo = ToLinear(input.Albedo.xyz);
    float3 normal = normalize(input.normal);
    float3 position = input.world;
    Mat mat;
    mat.Metalicness = 0.0;
    mat.Roughness = 0.4;
    mat.AmbientOcclusion = 1.0;
        
    float3 accumulator = float3(0, 0, 0);
        
    for (uint i = 0; i < 5; i++)
    {
        float3 L = normalize(lights[i]);
        accumulator += ComputeLight(albedo, normal, mat, position, CameraPosition, L, float4(1, 1, 1, 1.0f), Strength);
    }
    
    output.color = float4(accumulator, 1.0);
    return output;
}