Sources: https://github.com/YaakovDavis/VorticeImGui/tree/master/VorticeImGui
TODO:
Add a depth buffer 
    https://docs.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-depth-stencil

Handle memory
    https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines
    `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();` ?? 


## General

Rendering with command lists (https://developer.nvidia.com/sites/default/files/akamai/gamedev/docs/GDC_2013_DUDASH_DeferredContexts.pdf)
could speed things up IFF we adhere to all the ideas in that slideshow.