#ifndef __GAMMA
#define __GAMMA

static const float GAMMA = 2.2f;
static const float INVERSE_GAMMA = 0.45454545455f;

static float3 ToLinear(float3 v)
{
    return pow(abs(v.rgb), float3(GAMMA, GAMMA, GAMMA));
}

static float4 ToLinear(float4 v)
{
    float3 rgb = pow(abs(v.rgb), float3(GAMMA, GAMMA, GAMMA));
    return float4(rgb.rgb, v.a);
}

static float3 ToGamma(float3 v)
{
    return pow(abs(v.rgb), float3(INVERSE_GAMMA, INVERSE_GAMMA, INVERSE_GAMMA));
}

static float4 ToGamma(float4 v)
{
    float3 rgb = pow(abs(v.rgb), float3(INVERSE_GAMMA, INVERSE_GAMMA, INVERSE_GAMMA));
    return float4(rgb.rgb, v.a);
}

#endif
