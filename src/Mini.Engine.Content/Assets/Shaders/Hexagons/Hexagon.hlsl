#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Radians.hlsl"
#include "../Includes/Coordinates.hlsl"

struct InstanceData
{
    float3 position;
    uint s0;
    uint s1;
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
};

sampler TextureSampler : register(s0);

Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);

StructuredBuffer<InstanceData> Instances : register(t5);

static const float Step = 0.866025403784f; // sin(PI/3)
static const float InnerRadius = 0.4f;
static const float InX = InnerRadius * Step;
static const float InZ = InnerRadius * 0.5f;

static const float OuterRadius = 0.5f;
static const float OutX = OuterRadius * Step;
static const float OutZ = OuterRadius * 0.5f;

static const float BorderRadius = OuterRadius - InnerRadius;

static const float4 vInNorth = float4(0, 0, -InnerRadius, 1);
static const float4 vInNorthEast = float4(InX, 0, -InZ, 1);
static const float4 vInSouthEast = float4(InX, 0, InZ, 1);
static const float4 vInSouth = float4(0, 0, InnerRadius, 1);
static const float4 vInSouthWest = float4(-InX, 0, InZ, 1);
static const float4 vInNorthWest = float4(-InX, 0, -InZ, 1);

static const float4 vOutNorthNorthWest = vInNorth + normalize(vInNorth - vInNorthEast) * BorderRadius;
static const float4 vOutNorth = float4(0, 0, -OuterRadius, 1);
static const float4 vOutNorthNorthEast = vInNorth + normalize(vInNorth - vInNorthWest) * BorderRadius;

static const float4 vOutNorthEastNorth = vInNorthEast + normalize(vInNorthEast - vInSouthEast) * BorderRadius;
static const float4 vOutNorthEast = float4(OutX, 0, -OutZ, 1);
static const float4 vOutNorthEastEast = vInNorthEast + normalize(vInNorthEast - vInNorth) * BorderRadius;

static const float4 vOutSouthEastNorth = vInSouthEast + normalize(vInSouthEast - vInSouth) * BorderRadius;
static const float4 vOutSouthEast = float4(OutX, 0, OutZ, 1);
static const float4 vOutSouthEastSouth = vInSouthEast + normalize(vInSouthEast - vInNorthEast) * BorderRadius;

static const float4 vOutSouthSouthEast = vInSouth + normalize(vInSouth - vInSouthWest) * BorderRadius;
static const float4 vOutSouth = float4(0, 0, OuterRadius, 1);
static const float4 vOutSouthSouthWest = vInSouth + normalize(vInSouth - vInSouthEast) * BorderRadius;

static const float4 vOutSouthWestSouth = vInSouthWest + normalize(vInSouthWest - vInNorthWest) * BorderRadius;
static const float4 vOutSouthWest = float4(-OutX, 0, OutZ, 1);
static const float4 vOutSouthWestNorth = vInSouthWest + normalize(vInSouthWest - vInSouth) * BorderRadius;

static const float4 vOutNorthWestSouth = vInNorthWest + normalize(vInNorthWest - vInNorth) * BorderRadius;
static const float4 vOutNorthWest = float4(-OutX, 0, -OutZ, 1);
static const float4 vOutNorthWestNorth = vInNorthWest + normalize(vInNorthWest - vInSouthWest) * BorderRadius;

static const uint E_INNER = 0;
static const float4 InnerRing[] = 
{
    vInNorth,
    vInNorthEast,
    vInSouthEast,
    vInSouth,
    vInSouthWest,
    vInNorthWest
};

static const uint E_INNER_N = 0;
static const uint E_INNER_NE = 1;
static const uint E_INNER_SE = 2;
static const uint E_INNER_S = 3;
static const uint E_INNER_SW = 4;
static const uint E_INNER_NW = 5;

static const uint E_OUTER = 1;
static const float4 OuterRing[] =
{
    vOutNorth,
    vOutNorthNorthEast,

    vOutNorthEastNorth,
    vOutNorthEast,
    vOutNorthEastEast,

    vOutSouthEastNorth,
    vOutSouthEast,
    vOutSouthEastSouth,

    vOutSouthSouthEast,
    vOutSouth,
    vOutSouthSouthWest,

    vOutSouthWestSouth,
    vOutSouthWest,
    vOutSouthWestNorth,

    vOutNorthWestSouth,
    vOutNorthWest,
    vOutNorthWestNorth,

    vOutNorthNorthWest
};

static const uint E_OUTER_N = 0;
static const uint E_OUTER_N_NE = 1;

static const uint E_OUTER_NE_N = 2;
static const uint E_OUTER_NE = 3;
static const uint E_OUTER_NE_E = 4;

static const uint E_OUTER_SE_N = 5;
static const uint E_OUTER_SE = 6;
static const uint E_OUTER_SE_S = 7;

static const uint E_OUTER_S_SE = 8;
static const uint E_OUTER_S = 9;
static const uint E_OUTER_S_SW = 10;

static const uint E_OUTER_SW_S = 11;
static const uint E_OUTER_SW = 12;
static const uint E_OUTER_SW_N = 13;

static const uint E_OUTER_NW_S = 14;
static const uint E_OUTER_NW = 15;
static const uint E_OUTER_NN_N = 16;

static const uint E_OUTER_N_NW = 17;

float UnPack(uint sides, int index)
{
    uint mask = 3 << (index * 2);
    uint shifted = sides & mask;
    uint positive = shifted >> (index * 2);

    return positive - 1.0f;
}

float4 GetSideOffset(uint s0, uint s1, int index)
{
    float offset;
    if(index < 16)
    {
        offset = UnPack(s0, index);
    }
    else
    {
        offset = UnPack(s1, index - 16);
    }
    
    return float4(0, offset * 0.05f, 0, 0);
}

Vertex GetHexagonVertex(InstanceData data, uint vertexId)
{
    static const float4 Sequence[] = 
    {
        InnerRing[E_INNER_N],
        InnerRing[E_INNER_NE],
        InnerRing[E_INNER_NW],
        InnerRing[E_INNER_SE],
        InnerRing[E_INNER_SW],
        InnerRing[E_INNER_S]
    };

    Vertex output;
    output.position = float4(data.position, 0.0f)+ Sequence[vertexId];
    output.normal = float3(0, 1, 0);
    output.texcoord = output.position.xz;
    return output;
}

Vertex GetStripVertex(InstanceData data, uint vertexId)
{
    static const uint2 Sequence[] = 
    {
        uint2(E_OUTER, E_OUTER_N),
        uint2(E_OUTER, E_OUTER_N_NE),
        uint2(E_INNER, E_INNER_N),
        uint2(E_OUTER, E_OUTER_NE_N),
        uint2(E_INNER, E_INNER_NE),
        uint2(E_OUTER, E_OUTER_NE),
    };

    Vertex output;
    
    uint instance = vertexId / 6;
    uint2 seq = Sequence[vertexId % 6];
    if (seq.x == E_INNER)
    {
        uint index = (seq.y + instance) % 6;
        output.position = float4(data.position, 0.0f) + InnerRing[index];
        output.normal = float3(0, 1, 0);
        output.texcoord = output.position.xz;
    }
    else // E_OUTER
    {
        uint index = (seq.y + instance * 3) % 18;
        float4 offset = GetSideOffset(data.s0, data.s1, index);
        float4 position = OuterRing[index];

        output.position = float4(data.position, 0.0f) + position + offset;

        // TODO: somehow the opposite normal is not exactly the same, leading to lighting problems?

        float s = -sign(offset.y); // -1 -> 1, 0 -> 0, 1 -> -1
        float3 direction = normalize(float3(position.x, 0, position.z));
        // If s == 0 (the ground is flat) we lerp from 0,1,0 to 0,0,0 which when
        // renormalized is again 0,1,0. In the other case we get a normal reflecting off the hill/cliff
        float3 normal = lerp(float3(0,1, 0), s * direction, 0.5f);
        output.normal = normalize(normal);

        output.texcoord = output.position.xz;
    }

    return output;
}

PS_INPUT CreateVsOutput(Vertex vertex)
{
    PS_INPUT output;
    
    float3x3 rotation = (float3x3) World;

    output.position = mul(WorldViewProjection, vertex.position);
    output.previousPosition = mul(PreviousWorldViewProjection, vertex.position);
    output.currentPosition = output.position;
    
    output.world = mul(World, vertex.position).xyz;
    output.normal = normalize(mul(rotation, vertex.normal));
    output.texcoord = ScreenToTexture(mul((float4x2)World, vertex.texcoord).xy);

    return output;
}

#pragma VertexShader
PS_INPUT VsHexagon(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    Vertex vertex = GetHexagonVertex(Instances[instanceId], vertexId);
    return CreateVsOutput(vertex);
}

#pragma VertexShader
PS_INPUT VsStrip(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    uint offset = (instanceId % 6) * 6;
    Vertex vertex = GetStripVertex(Instances[instanceId / 6], vertexId + offset);
    return CreateVsOutput(vertex);
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
    output.normal = float4(PackNormal(input.normal), 1.0f);

    output.velocity = previousUv - currentUv;

    return output;
}