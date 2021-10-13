# TODO
Maybe parse mouse using https://github.com/MicrosoftDocs/win32/blob/docs/desktop-src/DxTechArts/taking-advantage-of-high-dpi-mouse-movement.md


Define internal model format

OBJ parser, for inspiration see
https://github.com/stefangordon/ObjParser

Output:
- one array with all vertices (position, texture, normal(?)...)
- one array with all indices 
- one array that describes all meshes (start index, number of indices)
- one array of matrices as they are now
- one array of matrices as they were originally, and their hierarchy

Goal as flat as possible, making it as easy as possible to render the entire model

# Notes
Particle renderer
https://miketuritzin.com/post/rendering-particles-with-compute-shaders/

Tips on handling raw memory using a shared pool `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();
https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines

Rendering with command lists could speed things up IFF we adhere to all the ideas in that slideshow.
https://developer.nvidia.com/sites/default/files/akamai/gamedev/docs/GDC_2013_DUDASH_DeferredContexts.pdf)

Due to the new flip mode and tearing support users need at least Windows 10 Anniversary edition (2016)
See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/variable-refresh-rate-displays