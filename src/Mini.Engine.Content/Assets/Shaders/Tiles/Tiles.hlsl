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

struct PS_LINE_INPUT
{
    float4 position : SV_POSITION;
    float4 previousPosition : POSITION0;
    float4 currentPosition : POSITION1;
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

static const float HEIGTH_DIFFERENCE = 1.f;

//  v1-------v3    NW-------NE
//  |  \      |    |         |
//  |    \    |    |         |
//  |      \  |    |         |
//  v0-------v2    SW-------SE

static const uint OUTLINE_VERTEX_ID_MAP[] =
{
    // top
    0, 1, 1, 3, 3, 2, 2, 0,
    // sides
    0, 0, 1, 1, 3, 3, 2, 2
};

static const float3 VERTEX_POSITION[] =
{
    // Flat (offset 0)
    float3(-1, 0, 1), // SW
    float3(-1, 0, -1), // NW
    float3(1, 0, 1), // SE
    float3(1, 0, -1), // NE
};

static const float3 VERTEX_HEIGHT_OFFSET[] =
{
    // Flat (offset 0)
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),

    // 1 Point up, NE (offset 4)
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, 0, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),

    // 1 side up, N (offet 8)
    float3(0, 0, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),
    float3(0, 0, 0),
    float3(0, HEIGTH_DIFFERENCE, 0),

    // 1 corner up NE, 1 corner down SW (offet 12)
    float3(0, -HEIGTH_DIFFERENCE, 0),
    float3(0, 0, 0),
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

     // 1 Point up, NE (offset 4)
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(0, 1, 0),
    float3(-0.2357022613286972, 0.9428090453147888, 0.2357022613286972), // NE

    // 1 side up, N (offet 8)
    float3(0, 1, 0),
    float3(0, 0.9701424241065979, 0.24253560602664948), // NW
    float3(0, 1, 0),
    float3(0, 0.9701424241065979, 0.24253560602664948), // NE

    // 1 corner up NE, 1 corner down SW (offet 12)
    float3(0, 1, 0),
    float3(-0.2357022613286972, 0.9428090453147888, 0.2357022613286972), // NW
    float3(0, 1, 0),
    float3(-0.2357022613286972, 0.9428090453147888, 0.2357022613286972), // NE
};

static const uint WALL_VERTEX_ID_MAP[] =
{
    1, 1,
    0, 0,
    2, 2,
    3, 3,
    1, 1
};

// Clockwise rotation over the y axis
float3x3 CreateRotationYClockWise(float radians)
{
    float c = (float) cos(-radians);
    float s = (float) sin(-radians);

    // [  c  0 -s  ]
    // [  0  1  0  ]
    // [  s  0  c  ]
    return float3x3(c, 0, s,    0, 1, 0,    -s, 0, c);
}

float3 GetPosition(uint vertexId, uint instanceId)
{
    uint type = Instances[instanceId].type;
    uint rotation = Instances[instanceId].rotation;

    float radians = rotation * PI_OVER_TWO;
    float3x3 mat  = CreateRotationYClockWise(radians);

    uint offset = type * 4 + vertexId;
    float3 position = VERTEX_POSITION[vertexId] + VERTEX_HEIGHT_OFFSET[offset];
    position = mul(mat, position);

    position.xz += ToTwoDimensional(instanceId, Columns) * 2.0f;
    position.y += Instances[instanceId].heigth * HEIGTH_DIFFERENCE;

    return position;
}

float3 GetNormal(uint vertexId, uint instanceId)
{
    uint type = Instances[instanceId].type;
    uint rotation = Instances[instanceId].rotation;

    float radians = rotation * PI_OVER_TWO;
    float3x3 mat  = CreateRotationYClockWise(radians);

    uint offset = type * 4 + vertexId;
    float3 normal = VERTEX_NORMALS[offset];

    return mul(mat, normal);
}

#pragma VertexShader
PS_INPUT Vs(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    float3 position = GetPosition(vertexId, instanceId);
    float3 normal = GetNormal(vertexId, instanceId);
    float2 texcoord = float2(position.x, position.z);

    float3x3 rotation = (float3x3) World;
    output.position = mul(WorldViewProjection, float4(position, 1));
    output.previousPosition = mul(PreviousWorldViewProjection, float4(position, 1));
    output.currentPosition = output.position;

    output.world = mul(World, float4(position, 1)).xyz;
    output.normal = normalize(mul(rotation, normal));
    output.texcoord = ScreenToTexture(mul((float4x2)World, texcoord).xy);

    return output;
}

#pragma VertexShader
PS_INPUT VsWall(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    uint cornerId = WALL_VERTEX_ID_MAP[vertexId];
    float3 position = GetPosition(cornerId, instanceId);
    position.y = position.y *  (vertexId % 2);

    float3 flatPosition = float3(position.x, 0, position.z);
    float3 flatCenterPosition = VERTEX_POSITION[cornerId];
    float3 sideNormal = normalize(flatPosition - flatCenterPosition);
    float3 topNormal = GetNormal(cornerId, instanceId);
    
    float3 normal = lerp(sideNormal, topNormal, clamp(position.y, 0, 1));
    
    float2 texcoord = float2(position.x + position.z, position.y);

    float3x3 rotation = (float3x3) World;
    output.position = mul(WorldViewProjection, float4(position, 1));
    output.previousPosition = mul(PreviousWorldViewProjection, float4(position, 1));
    output.currentPosition = output.position;

    output.world = mul(World, float4(position, 1)).xyz;
    output.normal = normalize(mul(rotation, normal));
    output.texcoord = ScreenToTexture(mul((float4x2)World, texcoord).xy);

    return output;
}

#pragma VertexShader
PS_DEPTH_INPUT VsWallDepth(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_DEPTH_INPUT output;

    uint cornerId = WALL_VERTEX_ID_MAP[vertexId];
    float3 position = GetPosition(cornerId, instanceId);
    position.y = position.y *  (vertexId % 2);

    output.position = mul(WorldViewProjection, float4(position, 1));
    output.world = output.position;

    return output;
}

#pragma VertexShader
PS_LINE_INPUT VsLine(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_LINE_INPUT output;

    uint cornerId = OUTLINE_VERTEX_ID_MAP[vertexId];

    float3 position = GetPosition(cornerId, instanceId);
    if (vertexId > 7)
    {
        position.y = position.y * (vertexId % 2);
    }

    position.y += 0.02f; // prevent depth inaccuracies causing jagged lines

    output.position = mul(WorldViewProjection, float4(position, 1));
    output.previousPosition = mul(PreviousWorldViewProjection, float4(position, 1));
    output.currentPosition = output.position;

    return output;
}

#pragma VertexShader
PS_DEPTH_INPUT VsDepth(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_DEPTH_INPUT output;

    float3 position = GetPosition(vertexId, instanceId);
    output.position = mul(WorldViewProjection, float4(position, 1));
    output.world = output.position;

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
OUTPUT Ps(PS_INPUT input)
{
    const uint steps = 3;
    float2 texcoords[] =
    {
        input.texcoord * 1.50942f,
        float2(sin(0.33f), cos(0.33f)) * 5 + input.texcoord * 0.80948f,
        float2(sin(0.75f), cos(0.75f)) * 3 + input.texcoord * 0.41235f ,
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

#pragma PixelShader
OUTPUT PsLine(PS_LINE_INPUT input)
{
    input.previousPosition /= input.previousPosition.w;
    input.currentPosition /= input.currentPosition.w;
    float2 previousUv = ScreenToTexture(input.previousPosition.xy - PreviousJitter);
    float2 currentUv = ScreenToTexture(input.currentPosition.xy - Jitter);

    OUTPUT output;
    output.albedo = float4(0, 0, 0, 1);
    output.material = float4(0, 0, 1, 1);
    output.normal = float4(PackNormal(float3(0, 1, 0)), 1);
    output.velocity = previousUv - currentUv;

    return output;
}

#pragma PixelShader
float PsDepth(PS_DEPTH_INPUT input) : SV_TARGET
{
    return input.world.z / input.world.w;
}