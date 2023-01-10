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

struct PNT
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

float UnPack(uint sides, int index)
{
    uint mask = 3 << (index * 2);
    uint shifted = sides & mask;
    uint positive = shifted >> (index * 2);

    return positive - 1.0f;
}

float4 GetSideOffset(uint sides, int index)
{
    float offset = UnPack(sides, index);
    return float4(0, offset * 0.05f, 0, 0);
}


PNT GetHexagonVertex(InstanceData data, uint vertexId)
{
    PNT output;
    output.position = float4(0, 0, 0, 1);
    output.normal = float3(0, 1, 0);
    output.texcoord = float2(0, 0);

    switch(vertexId)
    {
        // Inner Hexagon
        case 0:
            output.position = vInNorth;
            break;
        case 1:
            output.position = vInNorthEast;
            break;
        case 2:
            output.position = vInNorthWest;
            break;
        case 3:
            output.position = vInSouthEast;
            break;
        case 4:
            output.position = vInSouthWest;
            break;
        case 5:
            output.position = vInSouth;
            break;
    }

    output.position += float4(data.position, 0.0f);
    output.texcoord += output.position.xz;

    return output;
}

PNT GetStripVertex(InstanceData data, uint vertexId)
{
    PNT output;
    output.position = float4(0, 0, 0, 1);
    output.normal = float3(0, 1, 0);
    output.texcoord = float2(0, 0);

    switch(vertexId)
    {
        // North East Strip
        case 0:
            output.position = vOutNorth;
            break;
        case 1:
            output.position = vOutNorthNorthEast;
            break;
        case 2:
            output.position = vInNorth;
            break;
        case 3:
            output.position = vOutNorthEastNorth;
            break;
        case 4:
            output.position = vInNorthEast;
            break;
        case 5:
            output.position = vOutNorthEast;
            break;

        // East strip
        case 6:
            output.position = vOutNorthEast;
            break;
        case 7:
            output.position = vOutNorthEastEast;
            break;
        case 8:
            output.position = vInNorthEast;
            break;
        case 9:
            output.position = vOutSouthEastNorth;
            break;
        case 10:
            output.position = vInSouthEast;
            break;
        case 11:
            output.position = vOutSouthEast;
            break;

        // South East strip
        case 12:
            output.position = vOutSouthEast;
            break;
        case 13:
            output.position = vOutSouthEastSouth;
            break;
        case 14:
            output.position = vInSouthEast;
            break;
        case 15:
            output.position = vOutSouthSouthEast;
            break;
        case 16:
            output.position = vInSouth;
            break;
        case 17:
            output.position = vOutSouth;
            break;

        // South West strip
        case 18:
            output.position = vOutSouth;
            break;
        case 19:
            output.position = vOutSouthSouthWest;
            break;
        case 20:
            output.position = vInSouth;
            break;
        case 21:
            output.position = vOutSouthWestSouth;
            break;
        case 22:
            output.position = vInSouthWest;
            break;
        case 23:
            output.position = vOutSouthWest;
            break;

        // West strip
        case 24:
            output.position = vOutSouthWest;
            break;
        case 25:
            output.position = vOutSouthWestNorth;
            break;
        case 26:
            output.position = vInSouthWest;
            break;
        case 27:
            output.position = vOutNorthWestSouth;
            break;
        case 28:
            output.position = vInNorthWest;
            break;
        case 29:
            output.position = vOutNorthWest;
            break;

        // North West strip
        case 30:
            output.position = vOutNorthWest;
            break;
        case 31:
            output.position = vOutNorthWestNorth;
            break;
        case 32:
            output.position = vInNorthWest;
            break;
        case 33:
            output.position = vOutNorthNorthWest;
            break;
        case 34:
            output.position = vInNorth;
            break;
        case 35:
            output.position = vOutNorth;
            break;
    }

    output.position += float4(data.position, 0.0f);
    output.texcoord += output.position.xz;

    // TODO: add heights and normals

    return output;
}

PS_INPUT CreateVsOutput(PNT pnt)
{
    PS_INPUT output;

    float4 position = pnt.position;
    float3 normal = pnt.normal;
    float2 texcoord = pnt.texcoord;
    
    float3x3 rotation = (float3x3) World;

    output.position = mul(WorldViewProjection, position);
    output.previousPosition = mul(PreviousWorldViewProjection, position);
    output.currentPosition = output.position;
    
    output.world = mul(World, position).xyz;
    output.normal = normalize(mul(rotation, normal));
    output.texcoord = ScreenToTexture(mul((float4x2)World, texcoord).xy);

    return output;
}

#pragma VertexShader
PS_INPUT VsHexagon(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PNT pnt = GetHexagonVertex(Instances[instanceId], vertexId);
    return CreateVsOutput(pnt);
}

#pragma VertexShader
PS_INPUT VsStrip(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    uint offset = (instanceId % 6) * 6;
    PNT pnt = GetStripVertex(Instances[instanceId / 6], vertexId + offset);
    return CreateVsOutput(pnt);
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