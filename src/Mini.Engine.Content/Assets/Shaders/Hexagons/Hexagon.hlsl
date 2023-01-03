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

float GetSideOffset(uint sides, int index)
{
    uint mask = 3 << (index * 2);
    uint shifted = sides & mask;
    uint positive = shifted >> (index * 2);

    return positive - 1.0f;
}

float4 GetPosition(InstanceData data, uint vertexId)
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
    
    switch(vertexId)
    {
        // Top triangle
        case 0:
            return vInNorthWest;
        case 1:
            return vInNorth;
        case 2:
            return vInNorthEast;
        
        // Bottom triangle
        case 3:
            return vInSouthWest;
        case 4:
            return vInSouthEast;
        case 5:
            return vInSouth;

        // top right middle
        case 6:
            return vInNorthWest;
        case 7:
            return vInNorthEast;
        case 8:
            return vInSouthEast;

        // bottom left middle
        case 9:
            return vInSouthWest;
        case 10:
            return vInNorthWest;
        case 11:
            return vInSouthEast;

        // top right flap
        case 12:
            return vInNorth;
        case 13:
            return vOutNorth;
        case 14:
            return vInNorthEast;
        case 15:
            return vOutNorth;
        case 16:
            return vOutNorthEast;
        case 17:
            return vInNorthEast;

        // Right flap
        case 18:
            return vInNorthEast;
        case 19:
            return vOutNorthEast;
        case 20:
            return vInSouthEast;
        case 21:
            return vOutNorthEast;
        case 22:
            return vOutSouthEast;
        case 23:
            return vInSouthEast;

        // bottom right flap
        case 24:
            return vInSouthEast;
        case 25:
            return vOutSouthEast;
        case 26:
            return vInSouth;
        case 27:
            return vOutSouthEast;
        case 28:
            return vOutSouth;
        case 29:
            return vInSouth;

        // bottom left flap
        case 30:
            return vInSouth;
        case 31:
            return vOutSouth;
        case 32:
            return vInSouthWest;
        case 33:
            return vOutSouth;
        case 34:
            return vOutSouthWest;
        case 35:
            return vInSouthWest;
        
        // left flap
        case 36:
            return vInSouthWest;
        case 37:
            return vOutSouthWest;
        case 38:
            return vInNorthWest;
        case 39:
            return vOutSouthWest;
        case 40:
            return vOutNorthWest;
        case 41:
            return vInNorthWest;
        
        // top left flap
        case 42:
            return vInNorthWest;
        case 43:
            return vOutNorthWest;
        case 44:
            return vInNorth;
        case 45:
            return vOutNorthWest;
        case 46:
            return vOutNorth;
        case 47:
            return vInNorth;
        
        // TODO: add the connections between each flap
        // TODO: set height of each outer vertex
        default:
            return float4(0, 0, 0, 1);
    }
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
    
    float4 position = GetPosition(data, vertexId) + float4(data.position, 1);
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