#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Defines.hlsl"

// Inspired by
// - https://youtu.be/Ibe1JBF5i5Y?t=634
// - https://outerra.blogspot.com/2012/05/procedural-grass-rendering.html


struct InstanceData
{
    float4x4 world;    
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
    float3 world :  WORLD;
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
    float3 CameraForward;
    float Tilt;
};
    
StructuredBuffer<InstanceData> Instances : register(t0);

float2 Linear(float x0, float y0, float x1, float y1, float t)
{
    float x = (x1 - x0) * t + x0;
    float y = (y1 - y0) * t + y0;
    return float2(x, y);
}


float2 CubicBezier(float x0, float y0, float x1, float y1, float x2, float y2, float t)
{
    float x = pow(1 - t, 2) * x0 + 2 * (1 - t) * t * x1 + pow(t, 2) * x2;
    float y = pow(1 - t, 2) * y0 + 2 * (1 - t) * t * y1 + pow(t, 2) * y2;
    
    return float2(x, y);
}

// TODO: this is much more expensive than a bezier curve 
// but it keeps the length of the segments equal and gives a nice way
// to parameterize flexibility
// TODO: get the normal from the angle
void Segments(float segment, float targetAngle, float length, float stiffness, float totalSegments, 
              out float2 position, out float2 normal)
{
    float2 accum = float2(0, 0);    
    float angle = 0.0f;
    for (float i = 0; i <= segment; i += 1.0f)
    {
        float flexibility = 0.001f + (i / (totalSegments - 1.0f));
        float e = pow(2, stiffness * flexibility - stiffness);
        angle = (targetAngle / totalSegments) * (i + 1) * e;
        float2 dir = float2(sin(angle), cos(angle));
        accum += dir * length;

    }
    
    position = accum;
    normal = float2(sin(angle - PI_OVER_TWO), cos(angle - PI_OVER_TWO));
}
 

float GetTiltFromWind(float3 worldPos, float3 facing)
{
    //return Tilt;
    return PI / 2.0f;
}


#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
           
    float isEven = vertexId % 2 == 0;
    float isOdd = 1.0f - isEven;
    
    float3 facing = float3(1.0f, 0.0f, 0.0f);
    const float width = 0.06f;
    float2 step = float2(-facing.z, facing.x) * width;
    
    float si = (vertexId / 2.0f) * isEven + ((vertexId - 1.0f) / 2.0f) * isOdd;
    float s = si / 3.0f;
      
    float4x4 world = Instances[instanceId].world;
    float3 root = mul(world, float4(0, 0, 0, 0)).xyz;
    float tilt = GetTiltFromWind(root, facing);
    float2 p;
    float2 n;
    Segments(si, tilt, 0.33f, 3.0f, 4, p, n);
    
    float4 position = float4(p.x + isOdd * step.x, p.y, isOdd * step.y, 1.0f);
    float2 texcoord = float2(isOdd, 1.0f - s);
    
    position.xyz += root;
    
    float3x3 rotation = (float3x3) world;
    float3 normal = float3(n.x, n.y, 0.0f);
    normal = normalize(mul(rotation, normal));
    
    // TODO: pull the normal towards the camera a little bit??
    normal = normalize(lerp(normal, -CameraForward, 0.25f));
    //normal = -CameraForward;
    
    
    output.position = mul(mul(ViewProjection, world), position);
    output.world = mul(world, position).xyz;
    output.texcoord = texcoord;
    output.normal = normal;
    output.tint = Instances[instanceId].tint;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;        
    
    float metalicness = 0.0f;
    float roughness = 1.0f;
    float ambientOcclusion = 0.0f;
        
    output.albedo = ToLinear(float4(input.tint, 1.0f));
    //output.albedo = ToLinear(float4(150 / 255.0f, 223 / 255.0f, 51 / 255.0f, 1));
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(input.normal), 1.0f);

    return output;
}