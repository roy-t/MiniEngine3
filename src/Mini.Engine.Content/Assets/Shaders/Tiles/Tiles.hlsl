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
//  |         |    |         |
//  |         |    |         |
//  |         |    |         |
//  v0-------v2    SW-------SE

static const uint SW = 0;
static const uint NW = 1;
static const uint SE = 2;
static const uint NE = 3;


static const int3x3 CORNER_TO_NEIGHBOURING_CORNER[] =
{
    int3x3(int3(-1, 0, SE), int3(-1, 1, NE), int3(0, 1, NW)), //SW -> ...
    int3x3(int3(-1, 0, NE), int3(-1, -1, SE), int3(0, -1, SW)), //NW -> ...
    int3x3(int3(1, 0, SW), int3(1, 1, NW), int3(0, 1, NE)), //SE -> ...
    int3x3(int3(1, 0, NW), int3(1, -1, SW), int3(0, -1, SE)), //NE -> ...
};

static const uint VERTEX_ID_TO_CORNER[] =
{
    // rotation 0
    SW,
    NW,
    SE,
    NE,

    // rotaton 1
    NW,
    NE,
    SW,
    SE,

    // rotation 2
    NE,
    SE,
    NW,
    SW,

    // rotation 3
    SE,
    SW,
    NE,
    NW
};

static const uint CORNER_TO_VERTEX_ID[] =
{
    // rotation 0
    SW,
    NW,
    SE,
    NE,

    // rotation 1
    SE,
    SW,
    NE,
    NW,

    // rotation 2
    NE,
    SE,
    NW,
    SW,

    // rotation 3
    NW,
    NE,
    SW,
    SE
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

float3 AverageNormal(float3 normal, uint vertexId, uint instanceId)
{
    // vertexId = 1
    uint rotation = Instances[instanceId].rotation; // 2
    uint offset = rotation * 4 + vertexId; // 8 + 1 = 9
    uint corner = VERTEX_ID_TO_CORNER[offset];  // SE

    int2 position =  (int2)ToTwoDimensional(instanceId, Columns); // 0, 0
    
    // int3x3(int3(1, 0, SW), int3(1, 1, NW), int3(0, 1, NE)), //SE -> ...
    int3x3 neighbourOffsets = CORNER_TO_NEIGHBOURING_CORNER[corner];

    float3 accumulator = normal;
    for(uint i = 0; i < 3; i++)
    {
        // only int3(0, 1, NE) passes the test
        int3 neighbourOffset = neighbourOffsets[i];
        int2 neighbourPosition = position + neighbourOffset.xy;
        if (neighbourPosition.x >= 0 && neighbourPosition.x < (int)Columns &&
            neighbourPosition.y >= 0 && neighbourPosition.y < (int)Rows)
        {   
            uint neighbourInstanceId = ToOneDimensional((uint)neighbourPosition.x, (uint)neighbourPosition.y, Columns);
            uint neighbourRotation = Instances[neighbourInstanceId].rotation; // 0
            uint neighbourVertexId = CORNER_TO_VERTEX_ID[neighbourRotation * 4 + neighbourOffset.z];
            
            accumulator += GetNormal(neighbourVertexId, neighbourInstanceId);
        }
    }
    
    return normalize(accumulator);
}

#pragma VertexShader
PS_INPUT Vs(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    float3 position = GetPosition(vertexId, instanceId);
    float3 normal = GetNormal(vertexId, instanceId);

    normal = AverageNormal(normal, vertexId, instanceId);

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