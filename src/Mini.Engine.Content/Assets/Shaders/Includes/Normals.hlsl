#ifndef __NORMALS
#define __NORMALS

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

float3 PerturbNormal(Texture2D tex, sampler samp, float3 normal, float3 view, float2 uv)
{
    float3 map = UnpackNormal(tex.Sample(samp, uv).xyz).xyz;
    float3x3 tbn = CotangentFrame(normal, -view, uv);
    return mul(map, tbn);
}

#endif
