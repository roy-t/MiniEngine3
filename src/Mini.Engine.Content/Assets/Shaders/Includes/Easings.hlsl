#ifndef __EASINGS
#define __EASINGS

float EasOutQuad(float x)
{
    return 1 - (1 - x) * (1 - x);
}

#endif
