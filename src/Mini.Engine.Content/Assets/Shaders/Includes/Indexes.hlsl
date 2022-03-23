#ifndef __INDEXES
#define __INDEXES

uint ToOneDimensional(uint x, uint y, uint stride)
{
    return x + (stride * y);
}

uint2 ToTwoDimensional(uint i, uint stride)
{
    uint x = i % stride;
    uint y = i / stride;

    return uint2(x, y);
}

#endif
