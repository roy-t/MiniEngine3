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
    float3 CameraPosition;
    float2 WindDirection;
    float WindScroll;
};

// Todo: seperate into per clump and per blade buffer
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

// TODO: this is probably more expensive than a bezier curve 
// but it keeps the length of the segments equal and gives a nice way
// to parameterize flexibility
// TODO: flatten loop
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

float GetTiltFromWind(float3 worldPos, float2 facing)
{
    float influence = dot(WindDirection, facing); 
    float noise = FBM(worldPos.xz + float2(WindScroll * 4, 0.0f), 0.05f, 1.0f, 1.0f, 0.55f);
    noise = (1.0f + noise) / 2.0f; // from [-1..1] to [0..1]
    float tilt = (influence * noise) * PI_OVER_TWO;
    return 0.5f + tilt;
}

float2 Slerp(float2 p0, float2 p1, float t)
{
    float dotp = dot(normalize(p0), normalize(p1));
    if ((dotp > 0.9999) || (dotp < -0.9999))
    {
        if (t <= 0.5)
            return p0;
        return p1;
    }
    float theta = acos(dotp);
    float2 P = ((p0 * sin((1 - t) * theta) + p1 * sin(t * theta)) / sin(theta));
    return P;
}

float3 Slerp(float3 p0, float3 p1, float t)
{
    float dotp = dot(normalize(p0), normalize(p1));
    if ((dotp > 0.9999) || (dotp < -0.9999))
    {
        if (t <= 0.5)
            return p0;
        return p1;
    }
    float theta = acos(dotp);
    float3 P = ((p0 * sin((1 - t) * theta) + p1 * sin(t * theta)) / sin(theta));    
    return P;
}


float2 GetFacingDirection(InstanceData instance)
{
    float3 viewDirection = instance.position - CameraPosition;
    
    float r = instance.rotation;
    float2 facingDirection = float2(sin(r), cos(r));
    float2 viewTangent = normalize(float2(viewDirection.z, -viewDirection.x));
    float viewTangentDotFacing = dot(viewTangent, facingDirection);
                
    float l = (1.0f - abs(viewTangentDotFacing)) * 0.25f;
    //l = 0.0f;
    
    // TODO: this makes grass flip direction
    // when its very close to the view direction
    // so it either has to drastically flip left or right
    if (viewTangentDotFacing > 0)
    {
        float2 foo = normalize(lerp(facingDirection, viewTangent, l));
        r = -atan2(foo.y, foo.x);
    }
    else
    {
        float2 foo = normalize(lerp(facingDirection, -viewTangent, l));
        r = -atan2(foo.y, foo.x);
    }
    
    return float2(sin(r), cos(r));
}


void GetSpinePosition(uint vertexId, float tilt, out float2 position, out float2 normal)
{
    float isEven = vertexId % 2 == 0;
    float isOdd = vertexId % 2 != 0;
    
    float si = (vertexId / 2.0f) * isEven + ((vertexId - 1.0f) / 2.0f) * isOdd;
    float2 p;
    float2 n;
    //Segments(si, tilt, 0.33f, 3.0f, 4, p, n);
    
    //float segment, float targetAngle, float length, float stiffness, float totalSegments
    float segment = si;
    float targetAngle = tilt;
    float length = 0.33f;
    float stiffness = 3.0f;
    float totalSegments = 4;
    
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

float3 BiasNormal(float2 n, float3x3 rotation, float bias)
{
    float3 normal = float3(n.x, n.y, 0.0f);
    normal = normalize(mul(rotation, normal));
    normal = normalize(lerp(normal, float3(0, 1, 0), bias));
    
    return normal;
}

#pragma VertexShader
PS_INPUT VS(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
{
    PS_INPUT output;
    
    InstanceData data = Instances[instanceId];
    
    float2 facing = GetFacingDirection(data);
    float tilt = GetTiltFromWind(data.position, facing);
    
    float2 spinePosition;
    float2 spineNormal;
    
    GetSpinePosition(vertexId, tilt, spinePosition, spineNormal);
        
    const float halfWidth = 0.03f;
    float side = ((vertexId % 2 == 0) * 2) - 1.0f;
    
    float4 position = float4(spinePosition.x + (side * halfWidth), spinePosition.y, 0.0f, 1.0f);
    float2 texcoord = float2(side + 1.0f, 0.0f);
    
    float4x4 world = CreateMatrix(atan2(facing.y, facing.x), data.position, data.scale);
    float3x3 rotation = (float3x3) world;
    
    
    float3 normal = BiasNormal(spineNormal, rotation, 0.33f);
    
    output.position = mul(mul(ViewProjection, world), position);
    output.world = mul(world, position).xyz;
    output.texcoord = texcoord;
    output.normal = normal;
    output.tint = data.tint;

    return output;
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // TODO: double check if everything still works, especiall the facing bias that makes sure you never see thin sides
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    
           
//    float3 tint = Instances[instanceId].tint;
    
//    float isEven = vertexId % 2 == 0;
//    float isOdd = 1.0f - isEven;
        
//    float si = (vertexId / 2.0f) * isEven + ((vertexId - 1.0f) / 2.0f) * isOdd;
//    float s = si / 3.0f;
      
//    InstanceData data = Instances[instanceId]; 
        
//    float3 viewDirection = data.position - CameraPosition;
    
//    float r = data.rotation;
//    float2 facingDirection = float2(sin(r), cos(r));    
//    float2 viewTangent = normalize(float2(viewDirection.z, -viewDirection.x));
//    float viewTangentDotFacing = dot(viewTangent, facingDirection);
                
//    float l = (1.0f - abs(viewTangentDotFacing)) * 0.25f;
//    //l = 0.0f;
    
//    // TODO: this makes grass flip direction
//    // when its very close to the view direction
//    // so it either has to drastically flip left or right
//    if (viewTangentDotFacing > 0)
//    {
//        float2 foo = normalize(lerp(facingDirection, viewTangent, l));
//        r = -atan2(foo.y, foo.x);
//    }
//    else
//    {
//        float2 foo = normalize(lerp(facingDirection, -viewTangent, l));
//        r = -atan2(foo.y, foo.x);
//    }
    
    
//    float4x4 world = CreateMatrix(r, data.position, data.scale);
//    float3x3 rotation = (float3x3) world;
    
//    float3 worldPosition = mul(world, float4(0, 0, 0, 1)).xyz;
//    //float3 facingDirection = mul(rotation, float3(0, 0, -1));
//    float tilt = GetTiltFromWind(worldPosition, facingDirection);
//    float2 p;
//    float2 n;
//    Segments(si, tilt, 0.33f, 3.0f, 4, p, n);
    
//    const float width = 0.06f;
//    float4 position = float4(p.x + isOdd * width, p.y, 0.0f, 1.0f);
//    float2 texcoord = float2(isOdd, 1.0f - s);
    
//    // Pull the normals up a bit to shade them as a semi-uniform plane
//    float3 normal = float3(n.x, n.y, 0.0f);
//    normal = normalize(mul(rotation, normal));            
//    normal = normalize(lerp(normal, float3(0, 1, 0), 0.33f));    
    
//    output.position = mul(mul(ViewProjection, world), position);
//    output.world = mul(world, position).xyz;
//    output.texcoord = texcoord;
//    output.normal = normal;
//    output.tint = tint;

//    return output;
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
    //output.albedo = ToLinear(float4(150 / 255.0f, 223 / 255.0f, 51 / 255.0f, 1));
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.normal = float4(PackNormal(input.normal), 1.0f);

    return output;
}