// Inspired by
// - https://youtu.be/Ibe1JBF5i5Y
// - https://outerra.blogspot.com/2012/05/procedural-grass-rendering.html

#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Radians.hlsl"
#include "Includes/Wind.hlsl"

struct InstanceData
{
    float3 position;
    float rotation;
    float scale;
    float3 tint;
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
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
    float3 tint : COLOR0;
    float ambientOcclusion : COLOR1;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float4 normal : SV_Target2;
};

cbuffer Constants : register(b0)
{
    float4x4 ViewProjection;
    float3 CameraPosition;
    float3 GrassToSunVector;
    float2 WindDirection;
    float WindScroll;
};

static const uint NumVertices = 7;
static const uint MaxVertexIndex = 6;

static const float4 AmbientOcclusions = float4(0.2f, 0.4f, 1.0f, 1.0f);

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);

// Todo: seperate into per clump and per blade buffer
StructuredBuffer<InstanceData> Instances : register(t0);

float4x4 CreateMatrix(float yaw, float3 offset)
{
    float c = (float) cos(yaw);
    float s = (float) sin(yaw);

    // [  c  0 -s  0 ]
    // [  0  1  0  0 ]
    // [  s  0  c  0 ]
    // [  0  0  0  1 ]
    return float4x4(c, 0, s, offset.x, 0, 1, 0, offset.y, -s, 0, c, offset.z, 0, 0, 0, 1);
}

float IsLeftVertex(uint vertexId)
{
    return vertexId % 2 == 0;
}

float IsRightVertex(uint vertexId)
{
    return vertexId % 2 != 0;
}

float GetSegmentIndex(uint vertexId)
{
    return floor(vertexId / 2.0f);
}

float BiasRotationToCameraRotation(float3 position, float rotation)
{
    // TODO: bias towards cameran this way?
    // float2 bladeToCamera = normalize(position.xz - CameraPosition.xz);
    // float2 perp = float2(-bladeToCamera.y, bladeToCamera.x);
    // float2 forward = RotationToVector(rotation);
    // float dt = dot(perp, forward);

    // float strength = pow(abs(dt), 2);
    //return rotation + 0.8f * strength * sign(dt);

    //return rotation;
    // // TODO: looking straight down shows an ugly pattern
    // // maybe try to reduce the strength of this rotation by distance

    // ensure rotation is [-PI..PI]
    rotation = WrapRadians(rotation);
    // Make sure that we don't see thin sides of blades by making sure the
    // blades are always rotated at most PI/3 relative to the camera
    float rotationOffset = clamp(rotation / 3.0f, -PI / 2.0f, PI / 2.0f);

    float3 bladeToCamere = normalize(position - CameraPosition);

    float2 flat = normalize(-float2(bladeToCamere.z, bladeToCamere.x));
    return atan2(flat.y, flat.x) + rotationOffset;
}

void GetSpineVertex(uint vertexId, float2 pos, float length, float targetAngle, out float3 position, out float segmentAngle)
{
    static const float stiffness = 3.0f;

    float segmentLength = length / 3;

    float4 angles;
    angles[0] = 0.0f;
    angles[1] = targetAngle / 2.25f;
    angles[2] = targetAngle / 1.55f;
    angles[3] = targetAngle / 1.0f;

    float f = 1.25f * (1.0f + snoise(pos * 107) * 0.25f);
    float t = (WindScroll * f) + (snoise(pos * 109) * 10);
    float a = 0.1f;

    angles[1] += sin(t) * a;
    angles[2] += sin(t + 1.2f) * a;
    angles[3] += sin(t + 2.4f) * a;

    float3 positions[4];
    positions[0] = float3(0, 0, 0);
    positions[1] = positions[0] + float3(0, cos(angles[1]), -sin(angles[1])) * segmentLength;
    positions[2] = positions[1] + float3(0, cos(angles[2]), -sin(angles[2])) * segmentLength;
    positions[3] = positions[2] + float3(0, cos(angles[3]), -sin(angles[3])) * segmentLength;

    float segment = GetSegmentIndex(vertexId);

    position = positions[segment];
    segmentAngle = angles[segment];
}

float3 GetBorderOffset(uint vertexId)
{
    float3 perp = float3(1, 0, 0);

    float l = IsLeftVertex(vertexId);
    float r = IsRightVertex(vertexId);
    float t = vertexId == MaxVertexIndex; // the top vertex
    float nt = vertexId != MaxVertexIndex; // not the top vertex

    return perp * (l - r) * nt;
}

float2 GetTextureCoordinates(uint vertexId)
{
    float r = IsLeftVertex(vertexId);
    float t = vertexId == MaxVertexIndex; // the top vertex
    float nt = vertexId != MaxVertexIndex; // not the top vertex

    return float2(r * nt + 0.5f * t,   0.0f);
}

float3 GetBorderPosition(float3 position, float3 borderDirection)
{
    static const float halfBladeThickness = 0.015f;
    return position + (halfBladeThickness * borderDirection);
}

float3 GetBorderNormal(uint vertexId, float3 borderDirection, float nAngle)
{
    float t = vertexId == MaxVertexIndex; // the top vertex
    float nt = vertexId != MaxVertexIndex; // not the top vertex
    float s = borderDirection.x;
        
    // Grass blades are single sided, so we have to pick a side for the the normal
    // if a blade is standing straight up (nAngle == 0), the normal points (0, 0, -1)
    // if a blade is completely bended (nAngle == PI/2), the normal points (0, 1, 0)
        
    // TODO: bending the blade normal slightly to the side gives a more rounded look to
    // the grass blades, but leads to a weird artefact when computing the image based lighting
    return normalize(float3(s * 0.25f, sin(nAngle), -cos(nAngle)));    
}

float3 GetWorldNormal(float4x4 world, float3 normal)
{
    float3x3 rotation = (float3x3) world;

    //// Figure out which side of the grass blade is pointing
    //// most towards the sun. So that we can use the normal of that
    //// side to give the illusion that blades of grass have two sides
    float3 forward = mul(rotation, float3(0, 0, -1));
    float3 backward = mul(rotation, float3(0, 0, 1));

    float d0 = dot(GrassToSunVector, forward);
    float d1 = dot(GrassToSunVector, backward);            
        
    if (d0 > d1)
    {
        normal = mul(rotation, normal);
    }
    else
    {
        normal = mul(rotation, float3(normal.x, normal.y, -normal.z));
    }
    
    return normal;
}

#pragma VertexShader
PS_INPUT
    VS(
    uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    InstanceData data = Instances[instanceId];

    //data.rotation = PI_OVER_TWO;

    float2 facing = RotationToVector(data.rotation);
    static const float baseTilt = PI / 2.0f;
    float tilt = baseTilt + (GetWindPower(data.position, facing, WindDirection, WindScroll) * (PI / 3.0f));
    //tilt = 0.0f;
    float3 position;
    float nAngle;
    GetSpineVertex(vertexId, data.position.xz, data.scale, tilt, position, nAngle);

    float3 borderDirection = GetBorderOffset(vertexId);
    position = GetBorderPosition(position, borderDirection);
    float3 normal = GetBorderNormal(vertexId, borderDirection, nAngle);

    float2 texcoord = GetTextureCoordinates(vertexId);
    float3 tint = data.tint;

    float biasedRotation = BiasRotationToCameraRotation(data.position, data.rotation);
    float4x4 world = CreateMatrix(biasedRotation, data.position);
    float4x4 worldViewProjection = mul(ViewProjection, world);

    output.position = mul(worldViewProjection, float4(position, 1.0f));
    output.texcoord = texcoord;        
    output.normal = GetWorldNormal(world, normal);;

    output.tint = tint;
    output.ambientOcclusion = AmbientOcclusions[GetSegmentIndex(vertexId)];

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;

    float metalicness = 0.0f;
    float roughness = 0.375f;
    float ambientOcclusion = input.ambientOcclusion;
    float4 tint = ToLinear(float4(input.tint, 1.0f));

    output.albedo = Albedo.Sample(TextureSampler, input.texcoord) * tint;
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(input.normal), 1.0f);

    return output;
}

