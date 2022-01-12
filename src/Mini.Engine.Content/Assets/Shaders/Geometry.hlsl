#include "Includes/Gamma.hlsl"
//#include "Includes/Normals.hlsl" // TODO: move back once SRGB issue is solved!

float3 PackNormal(float3 normal)
{
    return 0.5f * (normalize(normal) + 1.0f);
}

float3 UnpackNormal(float3 normal)
{
    return normalize((2.0f * normal) - 1.0f);
}

// Normal mapping as described by Christian Schüler in
// http://www.thetenthplanet.de/archives/1180
float3x3 CotangentFrame(float3 N, float3 p, float2 uv)
{
    // get edge vectors of the pixel triangle
    float3 dp1 = ddx(p);
    float3 dp2 = ddy(p);
    float2 duv1 = ddx(uv);
    float2 duv2 = ddy(uv);

    // solve the linear system
    float3 dp2perp = cross(dp2, N);
    float3 dp1perp = cross(N, dp1);
    float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

    // construct a scale-invariant frame
    float invmax = rsqrt(max(dot(T, T), dot(B, B)));
    return float3x3(T * invmax, B * invmax, N);
}

float3 PerturbNormal(Texture2D tex, sampler samp, float3 normal, float3 view, float2 uv)
{    
    //float3 map = UnpackNormal(float3(0.5f, 0.5f, 1.0f));
    // TODO: because normal maps are marked as SRGB textures
    // DirectX automatically converts them to linear color space
    // this is incorrect so we undo it here, but ideally we 
    // would mark these textures correctly!
    float3 map = UnpackNormal(ToGamma(tex.Sample(samp, uv)).xyz);
    float3x3 tbn = CotangentFrame(normal, -view, uv);
    return normalize(mul(map, tbn));
}

struct VS_INPUT
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 screen : SCREEN;
    float3 world :  WORLD;
    float2 texcoord : TEXCOORD;
    float3 normal : NORMAL;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float depth : SV_Target2;
    float4 normal : SV_Target3;
};

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float Unused;
};

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    // TODO: might need to transpose world before taking the rotation matrix
    // and then swap the arguments to mul!
    float3x3 rotation = (float3x3)World;
    float4 position = float4(input.position, 1.0f);

    output.position = mul(WorldViewProjection, position);
    output.world = mul(World, position).xyz;
    output.normal = normalize(mul(rotation, input.normal));
    output.texcoord = input.texcoord;
    output.screen = output.position;

    return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
    float4 albedo = Albedo.Sample(TextureSampler, input.texcoord);
    clip(albedo.a - 1.0f);

    float3 V = normalize(CameraPosition - input.world);
    float3 normal = PerturbNormal(Normal, TextureSampler, input.normal, V, input.texcoord);
 
    float metalicness = Metalicness.Sample(TextureSampler, input.texcoord).r;
    float roughness = Roughness.Sample(TextureSampler, input.texcoord).r;
    float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, input.texcoord).r;

    OUTPUT output;
    output.albedo = albedo;
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.depth = input.screen.z / input.screen.w;
    output.normal = float4(PackNormal(normal), 1.0f);
    
    //output.normal = float4(Normal.Sample(TextureSampler, input.texcoord).xyz, 1); 
    //output.normal = float4(PackNormal(input.normal), 1.0f);
    //output.normal = float4(V, 1);

    // Checked
    // - input.normal & Packing
    // - CameraPosition
    // - input.world
    // - V
    // - Normal and TextureSampler

    // !!! its how the texture is read !!! Double gamma?

    return output;
}