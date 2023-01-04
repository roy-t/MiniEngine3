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
    
static const float STEP = (TWO_PI / 6.0f);
static const float INNER_RADIUS = 0.40f;
static const float OUTER_RADIUS = 0.5f;
static const float BORDER_RADIUS = OUTER_RADIUS - INNER_RADIUS;

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

float3 GetSideNormal(float4 innerPosition, float4 outerPosition)
{
    float angle = tan(outerPosition.y / distance(innerPosition, outerPosition));
    angle += PI_OVER_TWO;
    
    float2 horizontal = normalize(outerPosition.xz - innerPosition.xz);

    return float3(sin(angle) * horizontal.x, sin(angle), cos(angle) * horizontal.y);
}

PNT GetPosition(InstanceData data, uint vertexId)
{    
    static const float Step = 0.866025403784f; // sin(PI/3)
    static const float InnerRadius = 0.4f;
    static const float InX = InnerRadius * Step;
    static const float InZ = InnerRadius * 0.5f;
    
    static const float OuterRadius = 0.5f;
    static const float OutX = OuterRadius * Step;
    static const float OutZ = OuterRadius * 0.5f;

    static const float4 vInNorth = float4(0, 0, -InnerRadius, 1);
    static const float4 vInNorthEast = float4(InX, 0, -InZ, 1);
    static const float4 vInSouthEast = float4(InX, 0, InZ, 1);
    static const float4 vInSouth = float4(0, 0, InnerRadius, 1);
    static const float4 vInSouthWest = float4(-InX, 0, InZ, 1);
    static const float4 vInNorthWest = float4(-InX, 0, -InZ, 1);

    static const float4 vOutNorth = float4(0, 0, -OuterRadius, 1);
    static const float4 vOutNorthEast = float4(OutX, 0, -OutZ, 1);
    static const float4 vOutSouthEast = float4(OutX, 0, OutZ, 1);
    static const float4 vOutSouth = float4(0, 0, OuterRadius, 1);
    static const float4 vOutSouthWest = float4(-OutX, 0, OutZ, 1);
    static const float4 vOutNorthWest = float4(-OutX, 0, -OutZ, 1);
    
    PNT pnt;
    pnt.position = float4(0, 0, 0, 1);
    pnt.normal = float3(0, 1, 0);
    pnt.texcoord = float2(0, 0);

    switch(vertexId)
    {
        // North triangle
        case 0:
            pnt.position = vInNorthWest;
            break;
        case 1:
            pnt.position = vInNorth;
            break;
        case 2:
            pnt.position = vInNorthEast;
            break;
        
        // Middle north-east triangle
        case 3:
            pnt.position = vInSouthWest;
            break;
        case 4:
            pnt.position = vInSouthEast;
            break;
        case 5:
            pnt.position = vInSouth;
            break;

        // Middle south west triangle
        case 6:
            pnt.position = vInNorthWest;
            break;
        case 7:
            pnt.position = vInNorthEast;
            break;
        case 8:
            pnt.position = vInSouthEast;
            break;

        // South triangle
        case 9:
            pnt.position = vInSouthWest;
            break;
        case 10:
            pnt.position = vInNorthWest;
            break;
        case 11:
            pnt.position = vInSouthEast;
            break;

        // NE flap
        case 12:
            pnt.position = vInNorth;
            break;
        case 13:
            pnt.position = vOutNorth + GetSideOffset(data.sides, 0);
            pnt.normal = GetSideNormal(vInNorth, vOutNorth + GetSideOffset(data.sides, 0));
            break;
        case 14:
            pnt.position = vInNorthEast;
            break;
        case 15:
            pnt.position = vOutNorth + GetSideOffset(data.sides, 0);
            pnt.normal = GetSideNormal(vInNorth, vOutNorth + GetSideOffset(data.sides, 0));
            break;
        case 16:
            pnt.position = vOutNorthEast + GetSideOffset(data.sides, 0);
            pnt.normal = GetSideNormal(vInNorthEast, vOutNorthEast + GetSideOffset(data.sides, 0));
            break;
        case 17:
            pnt.position = vInNorthEast;
            break;

        // Right flap
        case 18:
            pnt.position = vInNorthEast;
            break;
        case 19:
            pnt.position = vOutNorthEast + GetSideOffset(data.sides, 1);
            pnt.normal = GetSideNormal(vInNorthEast, vOutNorthEast + GetSideOffset(data.sides, 1));
            break;
        case 20:
            pnt.position = vInSouthEast;
            break;
        case 21:
            pnt.position = vOutNorthEast + GetSideOffset(data.sides, 1);
            pnt.normal = GetSideNormal(vInNorthEast, vOutNorthEast + GetSideOffset(data.sides, 1));
            break;
        case 22:
            pnt.position = vOutSouthEast + GetSideOffset(data.sides, 1);
            pnt.normal = GetSideNormal(vInSouthEast, vOutSouthEast + GetSideOffset(data.sides, 1));
            break;
        case 23:
            pnt.position = vInSouthEast;
            break;

        // bottom right flap
        case 24:
            pnt.position = vInSouthEast;
            break;
        case 25:
            pnt.position = vOutSouthEast + GetSideOffset(data.sides, 2);
            pnt.normal = GetSideNormal(vInSouthEast, vOutSouthEast + GetSideOffset(data.sides, 2));
            break;
        case 26:
            pnt.position = vInSouth;
            break;
        case 27:
            pnt.position = vOutSouthEast + GetSideOffset(data.sides, 2);
            pnt.normal = GetSideNormal(vInSouthEast, vOutSouthEast + GetSideOffset(data.sides, 2));
            break;
        case 28:
            pnt.position = vOutSouth + GetSideOffset(data.sides, 2);
            pnt.normal = GetSideNormal(vInSouth, vOutSouth + GetSideOffset(data.sides, 2));
            break;
        case 29:
            pnt.position = vInSouth;
            break;

        // bottom left flap
        case 30:
            pnt.position = vInSouth;
            break;
        case 31:
            pnt.position = vOutSouth + GetSideOffset(data.sides, 3);
            pnt.normal = GetSideNormal(vInSouth, vOutSouth + GetSideOffset(data.sides, 3));
            break;
        case 32:
            pnt.position = vInSouthWest;
            break;
        case 33:
            pnt.position = vOutSouth + GetSideOffset(data.sides, 3);
            pnt.normal = GetSideNormal(vInSouth, vOutSouth + GetSideOffset(data.sides, 3));
            break;
        case 34:
            pnt.position = vOutSouthWest + GetSideOffset(data.sides, 3);
            pnt.normal = GetSideNormal(vInSouthWest, vOutSouthWest + GetSideOffset(data.sides, 3));
            break;
        case 35:
            pnt.position = vInSouthWest;
            break;
        
        // left flap
        case 36:
            pnt.position = vInSouthWest;
            break;
        case 37:
            pnt.position = vOutSouthWest + GetSideOffset(data.sides, 4);
            pnt.normal = GetSideNormal(vInSouthWest, vOutSouthWest + GetSideOffset(data.sides, 4));
            break;
        case 38:
            pnt.position = vInNorthWest;
            break;
        case 39:
            pnt.position = vOutSouthWest + GetSideOffset(data.sides, 4);
            pnt.normal = GetSideNormal(vInSouthWest, vOutSouthWest + GetSideOffset(data.sides, 4));
            break;
        case 40:
            pnt.position = vOutNorthWest + GetSideOffset(data.sides, 4);
            pnt.normal = GetSideNormal(vInNorthWest, vOutNorthWest + GetSideOffset(data.sides, 4));
            break;
        case 41:
            pnt.position = vInNorthWest;
            break;
        
        // top left flap
        case 42:
            pnt.position = vInNorthWest;
            break;
        case 43:
            pnt.position = vOutNorthWest + GetSideOffset(data.sides, 5);
            pnt.normal = GetSideNormal(vInNorthWest, vOutNorthWest + GetSideOffset(data.sides, 5));
            break;
        case 44:
            pnt.position = vInNorth;
            break;
        case 45:
            pnt.position = vOutNorthWest + GetSideOffset(data.sides, 5);
            pnt.normal = GetSideNormal(vInNorthWest, vOutNorthWest + GetSideOffset(data.sides, 5));
            break;
        case 46:
            pnt.position = vOutNorth + GetSideOffset(data.sides, 5);
            pnt.normal = GetSideNormal(vInNorth, vOutNorth + GetSideOffset(data.sides, 5));
            break;
        case 47:
            pnt.position = vInNorth;
            break;
        
        // N flap north wall
        case 48:
            pnt.position = vOutNorth + GetSideOffset(data.sides, 0);
            break;
        case 49:
            pnt.position = vInNorth;
            break;
        case 50:
            pnt.position = vOutNorth + GetSideOffset(data.sides, 5);
            break;

        // NE flap north wall
        case 51:
            pnt.position = vOutNorthEast + GetSideOffset(data.sides, 1);
            break;
        case 52:
            pnt.position = vInNorthEast;
            break;
        case 53:
            pnt.position = vOutNorthEast + GetSideOffset(data.sides, 0);
            break;

        // SE flap north wall
        case 54:
            pnt.position = vOutSouthEast + GetSideOffset(data.sides, 2);
            break;
        case 55:
            pnt.position = vInSouthEast;
            break;
        case 56:
            pnt.position = vOutSouthEast + GetSideOffset(data.sides, 1);
            break;

        // S flap north wall
        case 57:
            pnt.position = vOutSouth + GetSideOffset(data.sides, 3);
            break;
        case 58:
            pnt.position = vInSouth;
            break;
        case 59:
            pnt.position = vOutSouth + GetSideOffset(data.sides, 2);
            break;
        
        // SW flap north wall
        case 60:
            pnt.position = vOutSouthWest + GetSideOffset(data.sides, 4);
            break;
        case 61:
            pnt.position = vInSouthWest;
            break;
        case 62:
            pnt.position = vOutSouthWest + GetSideOffset(data.sides, 3);
            break;

        // NW flap north wall
        case 63:
            pnt.position = vOutNorthWest + GetSideOffset(data.sides, 5);
            break;
        case 64:
            pnt.position = vInNorthWest;
            break;
        case 65:
            pnt.position = vOutNorthWest + GetSideOffset(data.sides, 4);
            break;

        default:
            pnt.position = float4(0, 0, 0, 1);
            break;
    }

    return pnt;
}

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    InstanceData data = Instances[instanceId];
    
    PS_INPUT output;
    
    PNT pnt = GetPosition(data, vertexId);
    float4 position = pnt.position + float4(data.position, 1);
    float3 normal = pnt.normal;
    float2 texcoord = pnt.texcoord;
    
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
    float3 normal = input.normal; //PerturbNormal(Normal, TextureSampler, input.normal, V, input.texcoord);
 
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