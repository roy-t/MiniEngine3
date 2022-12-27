#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Radians.hlsl"
#include "../Includes/Coordinates.hlsl"

struct InstanceData
{
    float3 position;
    uint sides;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 previousPosition : POSITION0;
    float4 currentPosition : POSITION1;
    float3 world : WORLD;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
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
    float4x4 PreviousWorldViewProjection;
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float2 PreviousJitter;
    float2 Jitter;
};

sampler TextureSampler : register(s0);

Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);
    
StructuredBuffer<InstanceData> Instances : register(t5);
    
static const float STEP = (TWO_PI / 6.0f);
static const float INNER_RADIUS = 0.40f;
static const float OUTER_RADIUS = 0.5f;
static const float BORDER_RADIUS = OUTER_RADIUS - INNER_RADIUS;

float4 GetPosition(InstanceData data, uint vertexId)
{
    uint triangleId = vertexId / 3;
    uint triangleVertexId = vertexId % 3;

    float offset = triangleId * STEP;
    float radians = max(triangleVertexId - 1.0f, 0.0f) * STEP + offset;

    if (triangleId < 6)
    {
        float radius = min(triangleVertexId, 1.0f) * INNER_RADIUS;
        
        float x = sin(-radians) * radius + data.position.x;
        float y = data.position.y;
        float z = cos(-radians) * radius + data.position.z;

        return float4(x, y, z, 1);
    }    
    else if (triangleId < 12) // half connecting prism A
    {
        float radius = INNER_RADIUS + max(1.0f - triangleVertexId, 0) * BORDER_RADIUS;

        float x = sin(radians) * radius + data.position.x;
        float y = data.position.y;
        float z = cos(radians) * radius + data.position.z;
        
        return float4(x, y, z, 1);
    }
    else // half of connecting prism B
    {
        float radius = INNER_RADIUS + min(triangleVertexId, 1.0f) * BORDER_RADIUS;

        float x = sin(-radians) * radius + data.position.x;
        float y = data.position.y;
        float z = cos(-radians) * radius + data.position.z;
        
        return float4(x, y, z, 1);
    }

    return float4(0, 0, 0, 1);
}

float3 GetNormal(InstanceData data, uint vertexId)
{
    return float3(0, 1, 0);
}

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    InstanceData data = Instances[instanceId];
    
    PS_INPUT output;
    
    float4 position = GetPosition(data, vertexId);
    float3 normal = GetNormal(data, vertexId);

    float3x3 rotation = (float3x3) World;

    output.position = mul(WorldViewProjection, position);
    output.previousPosition = mul(PreviousWorldViewProjection, position);
    output.currentPosition = output.position;
    
    output.world = mul(World, position).xyz;
    output.normal = normalize(mul(rotation, normal));
    output.texcoord = ScreenToTexture(mul(World, position).xz);

    return output;

}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    float4 albedo = Albedo.Sample(TextureSampler, input.texcoord);
    clip(albedo.a - 0.5f);

    float3 V = normalize(CameraPosition - input.world);
    float3 normal = PerturbNormal(Normal, TextureSampler, input.normal, V, input.texcoord);
 
    float metalicness = Metalicness.Sample(TextureSampler, input.texcoord).r;
    float roughness = Roughness.Sample(TextureSampler, input.texcoord).r;
    float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, input.texcoord).r;

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