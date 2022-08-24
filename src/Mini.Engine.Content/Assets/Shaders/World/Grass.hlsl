#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"

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
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float4 normal : SV_Target2;
};

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float Tilt;
};
    
StructuredBuffer<float3> Instances : register(t0);

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


#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3)World;
    
    // from: https://youtu.be/Ibe1JBF5i5Y?t=634
    // and: https://outerra.blogspot.com/2012/05/procedural-grass-rendering.html
    //float tilt = 0.0f;
    float tilt = Tilt;
    //float tilt = 3.14f / 4.0f; // 45 deg
    
    float length = 1.0f;
    
    float3 facing = float3(1.0f, 0.0f, 0.0f);
    
    // TODO: make tip rotatable based on facing, and incorporate in other parameters
    float2 tip = float2(sin(tilt), cos(tilt));
    
    // Push
    //float2 midpoint = tip * 0.7f + (float2(-tip.x, tip.y) * 0.25f);
    //float2 midpoint = tip * 0.7f + (float2(-tip.x ));
    float2 midpoint = float2(tip.x * 0.125f, tip.y * 0.9f);
    
    
    float2 step = float2(-facing.z, facing.x) * 0.06f;
        
    float s = vertexId % 2 == 0
        ? (vertexId / 2.0f) 
        : (vertexId - 1.0f) / 2.0f;
    
    s /= 3.0f;
    s = min(1.0f, s);
    
    // TODO: something is wrong with the CubicBezier interpolation
    // as the segements keep changing length and the midpoint doesn't look 
    // to influence it as in desmos? Easy to see that the tip gets much longer!
    
    //float2 p = Linear(0, 0, tip.x, tip.y, s);
    float2 p = CubicBezier(0, 0, midpoint.x, midpoint.y, tip.x, tip.y, s);
    
    float4 position = vertexId % 2 == 0
        ? float4(p.x, p.y, 0.0f, 1.0f)
        : float4(p.x + step.x, p.y, step.y, 1.0f);
       
    float2 texcoord = vertexId % 2 == 0
        ? float2(0.0f, 1.0f - s)
        : float2(1.0f, 1.0f - s);
    
    
    output.position = mul(WorldViewProjection, position);
    output.world = mul(World, position).xyz;
    output.texcoord = texcoord;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    OUTPUT output;        
    
    float metalicness = 0.0f;
    float roughness = 1.0f;
    float ambientOcclusion = 0.0f;
        
    output.albedo = float4(0, 1, 0, 1);
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(float3(0, 1, 0)), 1.0f);

    return output;
}