#ifndef __RADIANS
#define __RADIANS

#include "Defines.hlsl"

/* wrap x -> [0,max) */
float WrapRadians(float radians, float max)
{    
    return fmod(max + fmod(radians, max), max);
}

/* wrap x -> [min,max) */
float WrapRadians(float radians, float min, float max)
{
    return min + WrapRadians(radians - min, max - min);
}

/* wrap x -> [-pi, pi) */
float WrapRadians(float radians)
{
    return WrapRadians(radians, -PI, PI);
}

// converts rotations to a vector where 0 -> (0, -1) aka forward
float2 RotationToVector(float radians)
{
    return float2(sin(radians), -cos(radians));
}

#endif
