#ifndef __SHADOWS
#define __SHADOWS

static const float BlendThreshold = 0.3f;
static const float Bias = 0.001f; // Bias to prevent shadow acne
static const uint NumCascades = 4;

struct ShadowProperties
{
    float4x4 ShadowMatrix;    
    float4x4 Offsets;
    float4x4 Scales;
    float4 Splits;
};

float SampleShadowMap(float2 baseUv, float u, float v, float2 shadowMapSizeInv, uint cascadeIndex, float z, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    float2 uv = baseUv + float2(u, v) * shadowMapSizeInv;
    return shadowMap.SampleCmpLevelZero(shadowSampler, float3(uv, cascadeIndex), z);
}

float SampleShadowMapPCF(float3 shadowPosition, uint cascadeIndex, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    float2 shadowMapSize;
    float _;
    shadowMap.GetDimensions(shadowMapSize.x, shadowMapSize.y, _);

    float lightDepth = shadowPosition.z - Bias;

    float2 uv = shadowPosition.xy * shadowMapSize;
    float2 shadowMapSizeInv = 1.0f / shadowMapSize;

    float2 baseUv;
    baseUv.x = floor(uv.x + 0.5f);
    baseUv.y = floor(uv.y + 0.5f);

    float s = (uv.x + 0.5f - baseUv.x);
    float t = (uv.y + 0.5f - baseUv.y);

    baseUv -= float2(0.5f, 0.5f);
    baseUv *= shadowMapSizeInv;

    float sum = 0.0f;
    
    float uw0 = (4 - 3 * s);
    float uw1 = 7;
    float uw2 = (1 + 3 * s);

    float u0 = (3 - 2 * s) / uw0 - 2;
    float u1 = (3 + s) / uw1;
    float u2 = s / uw2 + 2;

    float vw0 = (4 - 3 * t);
    float vw1 = 7;
    float vw2 = (1 + 3 * t);

    float v0 = (3 - 2 * t) / vw0 - 2;
    float v1 = (3 + t) / vw1;
    float v2 = t / vw2 + 2;

    sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw2 * vw0 * SampleShadowMap(baseUv, u2, v0, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);

    sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw2 * vw1 * SampleShadowMap(baseUv, u2, v1, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);

    sum += uw0 * vw2 * SampleShadowMap(baseUv, u0, v2, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw1 * vw2 * SampleShadowMap(baseUv, u1, v2, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);
    sum += uw2 * vw2 * SampleShadowMap(baseUv, u2, v2, shadowMapSizeInv, cascadeIndex, lightDepth, shadowMap, shadowSampler);

    return sum * 1.0f / 144;
}

float SampleShadowCascade(float3 shadowPosition, uint cascadeIndex, bool filter, ShadowProperties shadow, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    shadowPosition += shadow.Offsets[cascadeIndex].xyz;
    shadowPosition *= shadow.Scales[cascadeIndex].xyz;

    if (filter)
    {
        return SampleShadowMapPCF(shadowPosition, cascadeIndex, shadowMap, shadowSampler);
    }
    else
    {
        float lightDepth = shadowPosition.z - Bias;
        return shadowMap.SampleCmpLevelZero(shadowSampler, float3(shadowPosition.xy, cascadeIndex), lightDepth);
    }
}

uint GetCascadeIndex(float depth, ShadowProperties shadow)
{
    uint cascadeIndex = 0;
    [unroll]
    for (uint i = 0; i < NumCascades - 1; ++i)
    {
        [flatten]
        if (depth > shadow.Splits[i])
        {
            cascadeIndex = i + 1;
        }
    }

    return cascadeIndex;
}

// TODO: test if code is faster without the filter boolean
float ComputeLightFactorInternal(float3 worldPosition, float depth, bool filter, ShadowProperties shadow, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    float3 position = mul(shadow.ShadowMatrix, float4(worldPosition, 1.0f)).xyz;

    uint cascadeIndex = GetCascadeIndex(depth, shadow);

    float shadowVisibility = SampleShadowCascade(position, cascadeIndex, filter, shadow, shadowMap, shadowSampler);

    float nextSplit = shadow.Splits[cascadeIndex];
    float splitSize = cascadeIndex == 0 ? nextSplit : nextSplit - shadow.Splits[cascadeIndex - 1];
    float splitDist = (nextSplit - depth) / splitSize;

    [branch]
    if (splitDist <= BlendThreshold && cascadeIndex != NumCascades - 1)
    {
        float nextSplitVisibility = SampleShadowCascade(position, cascadeIndex + 1, filter, shadow, shadowMap, shadowSampler);
        float lerpAmt = smoothstep(0.0f, BlendThreshold, splitDist);
        shadowVisibility = lerp(nextSplitVisibility, shadowVisibility, lerpAmt);
    }

    return shadowVisibility;
}

float ComputeLightFactor(float3 worldPosition, float depth, ShadowProperties shadow, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    return ComputeLightFactorInternal(worldPosition, depth, false, shadow, shadowMap, shadowSampler);
}

float ComputeLightFactorPCF(float3 worldPosition, float depth, ShadowProperties shadow, Texture2DArray shadowMap, SamplerComparisonState shadowSampler)
{
    return ComputeLightFactorInternal(worldPosition, depth, true, shadow, shadowMap, shadowSampler);
}
#endif
