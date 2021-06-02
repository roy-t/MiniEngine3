Sources: https://github.com/YaakovDavis/VorticeImGui/tree/master/VorticeImGui
TODO:

https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-render-multi-thread-command-list-record

Integrate renderdoc?
https://github.com/mellinoe/veldrid/tree/master/src/Veldrid.RenderDoc

Handle memory
    https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines
    `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();` ?? 

# Plans for architecture

## Shaders
Auto generate shaders using a source code generator. Somehow derive the InputLayout and auto generate that as well. 
Maybe by always declaring the vertex shader input as the struct `VS_INPUT`?

I also want to auto generate structs and setters for the constant buffer(s).
Other than that it should be similar to what MiniEngine does.

## General

Is it worth it to get rid of all ID3D11 interfaces? What about device states and the device itself?
Maybe they could be hidden (internal fields) from everything outside the Mini.Engine.DirectX library?

