#include "../Includes/Normals.hlsl"

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
    float __Padding;
};

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
//Texture2D Metalicness : register(t2);
//Texture2D Roughness : register(t3);
//Texture2D AmbientOcclusion : register(t4);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3)World;
    float4 position = float4(input.position, 1.0f);

    output.position = mul(WorldViewProjection, position);
    output.world = mul(World, position).xyz;
    output.texcoord = input.texcoord;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;
    // TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO// TODO
    // TODO: use both normal map and vertex normal just like Geometry.hlsl, remember that the normal is not packed yet, fixed that in generator!
    
    float3 normal = Normal.Sample(TextureSampler, input.texcoord).xyz;
    
    output.albedo = float4(1, 0, 0, 1);// Albedo.Sample(TextureSampler, input.texcoord);
    output.normal = float4(PackNormal(normal), 1.0f);
    output.material = float4(0.0f, 0.0f, 0.0f, 1.0f);


    //float4 albedo = Albedo.Sample(TextureSampler, input.texcoord);
    //clip(albedo.a - 0.5f);

    //float3 V = normalize(CameraPosition - input.world);
    //float3 normal = PerturbNormal(Normal, TextureSampler, input.normal, V, input.texcoord);
 
    //float metalicness = Metalicness.Sample(TextureSampler, input.texcoord).r;
    //float roughness = Roughness.Sample(TextureSampler, input.texcoord).r;
    //float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, input.texcoord).r;

    
    //output.albedo = float4(1, 0, 0, 1);
    //output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    //output.normal = float4(PackNormal(normal), 1.0f);

    return output;
}