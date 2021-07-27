Sources: https://github.com/YaakovDavis/VorticeImGui/tree/master/VorticeImGui
TODO:

Add a depth buffer 
    https://docs.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-depth-stencil

Handle memory
    https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines
    `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();` ?? 

# Plans for architecture

## Shaders
Use View -> Other Windows -> Shader Syntax Visualizer to help create the right code!


Auto generate shaders using a source code generator. Somehow derive the InputLayout and auto generate that as well. 
Maybe by always declaring the vertex shader input as the struct `VS_INPUT`?

Shader class mimics an effect now. Maybe simplify/split it so that we can do `context.VS.SetShader(...)` and `context.PS.SetShader(...)`

## General

Is it worth it to get rid of all ID3D11 interfaces? What about device states and the device itself?
Maybe they could be hidden (internal fields) from everything outside the Mini.Engine.DirectX library?

Rendering with command lists (https://developer.nvidia.com/sites/default/files/akamai/gamedev/docs/GDC_2013_DUDASH_DeferredContexts.pdf)
could speed things up IFF we adhere to all the ideas in that slideshow.