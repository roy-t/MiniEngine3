#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Defines.hlsl"
#include "Includes/Wind.hlsl"

// Inspired by
// - https://youtu.be/Ibe1JBF5i5Y?t=634
// - https://outerra.blogspot.com/2012/05/procedural-grass-rendering.html


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
    float3 CameraForward;
    float AspectRatio;
    float2 WindDirection;
    float WindScroll;
};

// Todo: seperate into per clump and per blade buffer
StructuredBuffer<InstanceData> Instances : register(t0);

float2 RotationToVector(float rotation)
{
    return float2(sin(rotation), -cos(rotation));
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
    return floor(vertexId / 2);
}

void GetSpineVertex(uint vertexId, float length, float targetAngle, out float3 position, out float segmentAngle)
{
    static const float stiffness = 3.0f;
    
    float segmentLength = length / 3;
    
    float4 angles;
    angles[0] = 0.0f;
    angles[1] = targetAngle / 4.0f;
    angles[2] = targetAngle / 2.0f;
    angles[3] = targetAngle / 1.0f;
        
    float3 positions[4];
    positions[0] = float3(0, 0, 0);
    positions[1] = positions[0] + float3(0, cos(angles[1]), -sin(angles[1])) * segmentLength;
    positions[2] = positions[1] + float3(0, cos(angles[2]), -sin(angles[2])) * segmentLength;
    positions[3] = positions[2] + float3(0, cos(angles[3]), -sin(angles[3])) * segmentLength;
    
    float segment = GetSegmentIndex(vertexId);
    
    position = positions[segment];
    segmentAngle = angles[segment];
}

void GetBorderVertex(uint vertexId, float2 facing, float nAngle, inout float3 position, inout float3 normal)
{
    static const float halfBladeThickness = 0.03f;
    
    float3 forward = float3(facing.x, 0.0f, facing.y);
    float3 perp = cross(forward, float3(0, 1, 0));
    
    float l = IsLeftVertex(vertexId);
    float r = IsRightVertex(vertexId);
    float t = vertexId == 6; // the top vertex
    float nt = vertexId != 6; // not the top vertex
    
    float direction = (l - r) * nt;
    
    position = position + (perp * halfBladeThickness * direction);
    
    // Grass blades are single sided, so we have to pick a side for the the normal
    // using -PI_OVER_TWO the normal is correct if the blade is facing away from you
    float3 n = float3(0, cos(nAngle - PI_OVER_TWO), -sin(nAngle - PI_OVER_TWO));
    
    // Slightly tilt the normal outwards to give a more 3D effect
    float3 target = normalize((n * t) + (perp * direction * nt));
    normal = normalize(lerp(n, target, 0.15f));    
}

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
    
    InstanceData data = Instances[instanceId];
        
    float2 facing = RotationToVector(data.rotation);    
    static const float baseTilt = PI / 4;
    float tilt = baseTilt + GetTiltFromWind(data.position, facing, WindDirection, WindScroll);
        
    float3 position;
    float3 normal;
    float nAngle;
    
    // TODO: all grass leaves are tilted in the same direction irregardless of rotation
    // but because wind is based on the direction of the grass, this makes things sweep
    // in weird directions. Somehow the overal effect is quite nice, but let's try to fix
    // this so that leaves tilt in the direction of their orientation
    // The bug is in GetSpineVertex as it doesn't care about the rotation
    // maybe we should just simplify spine and border vertex and create a rotation matrix at the end?
    // Best seen with the 11 test things and tilt to 3.0f fixed value!
    GetSpineVertex(vertexId, data.scale, tilt, position, nAngle);        
    GetBorderVertex(vertexId, facing, nAngle, position, normal);
        
    position += data.position;
    float2 texcoord = float2(0, 0);
    float3 tint = data.tint;
        
    output.position = mul(ViewProjection, float4(position, 1.0f));
    output.texcoord = texcoord;
    output.normal = normal;
    output.tint = tint;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;        
    
    float metalicness = 0.0f;
    float roughness = 0.4f;
    // TODO: ambient occlusion is somewhere done wrong 
    // as setting it to 0.0 still lights things
    float ambientOcclusion = 1.0f;
    
    output.albedo = ToLinear(float4(input.tint, 1.0f));
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(input.normal), 1.0f);

    return output;
}

