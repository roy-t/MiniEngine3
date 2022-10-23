#include "Includes/BRDF.hlsl"
#include "Includes/Lights.hlsl"

RWTexture2D<float2> Lut : register(u0);
    
cbuffer Constants : register(b0)
{
    uint Width;
    uint Heigth;
}

#pragma ComputeShader
[numthreads(8, 8, 1)]
void BrdfLutKernel(in uint3 dispatchId : SV_DispatchThreadID)
{
    if (dispatchId.x >= Width || dispatchId.y >= Heigth)
    {
        return;
    }
	
    float rx = dispatchId.x / (float) Width;
    float ry = dispatchId.y / (float) Heigth;

    float NdotV = rx;
    float roughness = 1.0f - ry;

    float3 V;
    V.x = sqrt(1.0f - NdotV * NdotV);
    V.y = 0.0f;
    V.z = NdotV;

    float A = 0.0f;
    float B = 0.0f;

    float3 N = float3(0.0f, 0.0f, 1.0f);

    const uint SAMPLE_COUNT = 1024u;
    for (uint i = 0u; i < SAMPLE_COUNT; i++)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, roughness);
        float3 L = normalize(2.0f * dot(V, H) * H - V);

        float NdotL = max(L.z, 0.0f);
        float NdotH = max(H.z, 0.0f);
        float VdotH = max(dot(V, H), 0.0f);

        if (NdotL > 0.0f)
        {
            float G = GeometrySmithIBL(N, V, L, roughness);
            float G_Vis = (G * VdotH) / (NdotH * NdotV);
            float Fc = pow(1.0f - VdotH, 5.0f);

            A += (1.0f - Fc) * G_Vis;
            B += Fc * G_Vis;
        }
    }

    A /= (float) SAMPLE_COUNT;
    B /= (float) SAMPLE_COUNT;

    Lut[dispatchId.xy] = float2(A, B);
}