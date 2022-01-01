#include "BRDF.hlsl"

struct VS_INPUT
{
	float3 position : POSITION;
    float2 texcoord : TEXCOORD;
};

struct PS_INPUT
{
	float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

struct OUTPUT
{
	float2 brdf : SV_Target;
};

#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output;

	output.position = float4(input.position, 1.0f);
	output.texcoord = float2(input.texcoord.x, 1.0f - input.texcoord.y);

	return output;
}

#pragma PixelShader
OUTPUT PS(PS_INPUT input)
{
	OUTPUT output;

	float NdotV = input.texcoord.x;
	float roughness = input.texcoord.y;

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

	output.brdf = float2(A, B);
	return output;
}