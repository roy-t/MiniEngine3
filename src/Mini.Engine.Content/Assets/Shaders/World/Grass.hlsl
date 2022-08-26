#include "../Includes/Normals.hlsl"
#include "../Includes/Gamma.hlsl"
#include "../Includes/Defines.hlsl"
#include "Includes/SimplexNoise.hlsl"

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
    float3 WindDirection;
    float WindScroll;
};
    
StructuredBuffer<InstanceData> Instances : register(t0);

float2 Linear(float x0, float y0, float x1, float y1, float t)
{
    float x = (x1 - x0) * t + x0;
    float y = (y1 - y0) * t + y0;
    return float2(x, y);
}


float4x4 CreateMatrix(float yaw, float3 offset, float scale)
{     
    float c = (float) cos(yaw);
    float s = (float) sin(yaw);
 
    // [  c  0 -s  0 ]
    // [  0  1  0  0 ]
    // [  s  0  c  0 ]
    // [  0  0  0  1 ]
    return transpose(float4x4(c * scale, 0, -s, 0, 0, scale, 0, 0, s, 0, c * scale, 0, offset.x, offset.y, offset.z, 1));
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
 

static const float2x2 m2 = float2x2(0.80, 0.60, -0.60, 0.80);
float FBM(float2 coord, float frequency, float amplitude, float lacunarity, float persistance)
{
    float sum = 0.0f;   
    for (int i = 0; i < 3; i++)
    {
        float noise = snoise(coord * frequency) * amplitude;
        frequency *= lacunarity;
        amplitude *= persistance;

        coord = frequency * mul(m2, coord);
        
        sum += noise;
    }
    
    return sum;
}


float GetTiltFromWind(float3 worldPos, float3 facing)
{
    float influence = dot(WindDirection, facing); 
    float noise = FBM(worldPos.xz + float2(WindScroll * 4, 0.0f), 0.05f, 1.0f, 1.0f, 0.55f);
    noise = (1.0f + noise) / 2.0f; // from [-1..1] to [0..1]
    float tilt = (influence * noise) * PI_OVER_TWO;
    return 0.5f + tilt;
}

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
           
    float isEven = vertexId % 2 == 0;
    float isOdd = 1.0f - isEven;
        
    float si = (vertexId / 2.0f) * isEven + ((vertexId - 1.0f) / 2.0f) * isOdd;
    float s = si / 3.0f;
      
    InstanceData data = Instances[instanceId];   
    float4x4 world = CreateMatrix(data.rotation, data.position, data.scale);
    float3x3 rotation = (float3x3) world;
    
    float3 worldPosition = mul(world, float4(0, 0, 0, 1)).xyz;
    float3 facingDirection = mul(rotation, float3(0, 0, -1));
    float tilt = GetTiltFromWind(worldPosition, facingDirection);
    float2 p;
    float2 n;
    Segments(si, tilt, 0.33f, 3.0f, 4, p, n);
    
    const float width = 0.06f;
    float4 position = float4(p.x + isOdd * width, p.y, 0.0f, 1.0f);
    float2 texcoord = float2(isOdd, 1.0f - s);
    
    // Pull the normals up a bit to shade them as a semi-uniform plane
    float3 normal = float3(n.x, n.y, 0.0f);
    normal = normalize(mul(rotation, normal));            
    normal = normalize(lerp(normal, float3(0, 1, 0), 0.33f));    
    
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
    float ambientOcclusion = 1.0f;
        
    output.albedo = ToLinear(float4(input.tint, 1.0f));
    //output.albedo = ToLinear(float4(150 / 255.0f, 223 / 255.0f, 51 / 255.0f, 1));
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(input.normal), 1.0f);

    return output;
}