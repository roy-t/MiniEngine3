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
    float3 world :  WORLD;
    float4 screen : SCREEN;
};

struct OUTPUT
{
    float4 albedo : SV_Target0;
    float4 material : SV_Target1;
    float depth : SV_Target2;
    float4 normal : SV_Target3;
};

cbuffer vertexBuffer : register(b0)
{
    float4x4 WorldViewProjection;
    float4x4 World;
    float3 CameraPosition;
    float unused0;
};

sampler TextureSampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Normal : register(t1);
Texture2D Metalicness : register(t2);
Texture2D Roughness : register(t3);
Texture2D AmbientOcclusion : register(t4);

float4 PackNormal(float3 normal)
{
    return float4(0.5f * (normalize(normal) + 1.0f), 1.0f);
}

float4 UnpackNormal(float3 normal)
{
    return float4(normalize((2.0f * normal) - 1.0f), 1.0f);
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

float3 PerturbNormal(float3 normal, float3 view, float2 uv)
{
    float3 map = UnpackNormal(Normal.Sample(TextureSampler, uv).xyz).xyz;
    float3x3 tbn = CotangentFrame(normal, -view, uv);
    return mul(map, tbn);
}

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    float3x3 rotation = (float3x3) World;
    float4 position = float4(input.position.xyz, 1.0f);

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
    float3 normal = Normal.Sample(TextureSampler, input.texcoord).rgb;
    normal = PerturbNormal(input.normal, V, input.texcoord);

    float metalicness = Metalicness.Sample(TextureSampler, input.texcoord).r;
    float roughness = Roughness.Sample(TextureSampler, input.texcoord).r;
    float ambientOcclusion = AmbientOcclusion.Sample(TextureSampler, input.texcoord).r;

    OUTPUT output;
    output.albedo = albedo;
    output.material = float4(metalicness, roughness, ambientOcclusion, 1.0f);
    output.depth = input.screen.z / input.screen.w;
    output.normal = PackNormal(input.normal);

    return output;
}