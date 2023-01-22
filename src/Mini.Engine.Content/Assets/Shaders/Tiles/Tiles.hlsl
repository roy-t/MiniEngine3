#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Radians.hlsl"
#include "../Includes/Coordinates.hlsl"
#include "../Includes/Indexes.hlsl"

struct InstanceData
{
    uint type;
    uint heigth;
    uint rotation;
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

struct Vertex
{
    float4 position;
    float3 normal;
    float2 texcoord;
};

cbuffer Constants : register(b0)
{
    float4x4 PreviousWorldViewProjection;
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float2 PreviousJitter;
    float2 Jitter;
    uint Columns;
    uint Rows;
};

sampler TextureSampler : register(s0);

Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);

StructuredBuffer<InstanceData> Instances : register(t5);

// We have 4 different types of tiles
// - flat
// - 1 point up
// - 1 side up
// - 1 corner up, 1 corner down

static const float HEIGTH_DIFFERENCE = 0.5f;

static const float3 VERTEX_POSITION[] =
{
    // Flat (offset 0)
    float3(1, 0, -1),
    float3(1, 0, 1),
    float3(-1, 0, 1),
    float3(-1, 0, -1),

    // 1 Point up, NW (offset 4)
    float3(1, 0, -1),
    float3(1, 0, 1),
    float3(-1, 0, 1),
    float3(-1, 0, -1),

    // 1 side up, N (offet 8)
    float3(1, 0, -1),
    float3(1, 0, 1),
    float3(-1, 0, 1),
    float3(-1, 0, -1),

    // 1 corner up NW, 1 corner down SE (offet 12)
    float3(1, 0, -1),
    float3(1, -0, 1),
    float3(-1, 0, 1),
    float3(-1, 0, -1),
};

static const float3 VERTEX_HEIGHT_OFFSET[] =
{
    // Flat (offset 0)
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),

    // 1 Point up, NW (offset 4)
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0.2357022613286972, 0.2357022613286972, 0.9428090453147888),

    // 1 side up, N (offet 8)
    float3(0, HEIGTH_DIFFERENCE, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),

    // 1 corner up NW, 1 corner down SE (offet 12)
    float3(0, 0, 0),
    float3(0, -HEIGTH_DIFFERENCE, 0),
    float3(0, 0, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),
};

static const float3 VERTEX_NORMALS[] =
{
    // Flat (offset 0)
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, 1, 0),

    // 1 Point up, NW (offset 4)
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),

    // 1 side up, N (offet 8)
    float3(0, HEIGTH_DIFFERENCE, 0),
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),

    // 1 corner up NW, 1 corner down SE (offet 12)
    float3(0, 1, 0),
    float3(0, -HEIGTH_DIFFERENCE, 0),
    float3(0, 1, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),
};

#pragma VertexShader
PS_INPUT Vs(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
    
    float3x3 rotation = (float3x3) World;
    
    Vertex vertex;

    // HACK: control type of triangle and rotation
    uint instanceType = 3;// see list of types at top of file
    uint instanceRotation = 0; // clockwise as seen from above


    uint triangleVertexId = vertexId % 3;
    uint triangleOffset = (vertexId / 3) * 2; 

    uint offset = (triangleVertexId + triangleOffset - instanceRotation) % 4;
    uint heightOffset = (offset - instanceRotation) % 4;
    uint typeOffset = instanceType * 4;

    float3 position = VERTEX_POSITION[offset + typeOffset] + VERTEX_HEIGHT_OFFSET[heightOffset + typeOffset];

    vertex.position = float4(position, 1.0f);
    vertex.position.xz += ToTwoDimensional(instanceId, Columns) * 2.0f;
    
    vertex.normal = float3(0, 1, 0);
    vertex.texcoord = float2(vertex.position.x, vertex.position.z);
    
    output.position = mul(WorldViewProjection, vertex.position);
    output.previousPosition = mul(PreviousWorldViewProjection, vertex.position);
    output.currentPosition = output.position;
    
    output.world = mul(World, vertex.position).xyz;
    output.normal = normalize(mul(rotation, vertex.normal));
    output.texcoord = ScreenToTexture(mul((float4x2)World, vertex.texcoord).xy);

    return output;
}

struct MultiUv
{
    float4 albedo;
    float metalicness;
    float roughness;
    float ambientOcclusion;
    float3 normal;
};

MultiUv SampleTextures(float3 world, float2 texCoord, float3 heightMapNormal)
{
    float4 albedo = Albedo.Sample(TextureSampler, texCoord);
    float metalicness = Metalicness.Sample(TextureSampler, texCoord).r;
    float roughness = Roughness.Sample(TextureSampler, texCoord).r;
    float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, texCoord).r;

    float3 V = normalize(CameraPosition - world);
    float3 normal = PerturbNormal(Normal, TextureSampler, heightMapNormal, V, texCoord);

    MultiUv output;
    output.albedo = albedo;
    output.metalicness = metalicness;
    output.roughness = roughness;
    output.ambientOcclusion = ambientOcclusion;
    output.normal = normal;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    const uint steps = 3;
    float2 texcoords[] =
    {
        input.texcoord * 83.0f,
        float2(sin(0.33f) * input.texcoord.x, cos(0.33f) * input.texcoord.y) * 53.0f,
        float2(sin(0.75f) * input.texcoord.x, cos(0.75f) * input.texcoord.y) * 1.0f + float2(0.33, 0.16f)
    };

    float sumWeigth = 0.0f;

    float4 albedo = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float metalicness = 0.0f;
    float roughness = 0.0f;
    float ambientOcclusion = 0.0f;
    float3 normal = float3(0.0f, 0.0f, 0.0f);

    [unroll]
    for(uint i = 0; i < steps; i++)
    {
        float weight = steps / (i + 2.0f);
        sumWeigth += weight;

        MultiUv layer = SampleTextures(input.world, texcoords[i], input.normal);
        albedo += layer.albedo * weight;
        metalicness += layer.metalicness * weight;
        roughness += layer.roughness * weight;
        ambientOcclusion += layer.ambientOcclusion * weight;
        normal += layer.normal * weight;
    }
    
    albedo /= sumWeigth;
    metalicness /= sumWeigth;
    roughness /= sumWeigth;
    ambientOcclusion /= sumWeigth;
    normal = normalize(normal / sumWeigth);

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