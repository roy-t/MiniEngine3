# TODO
https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-flip-model
1. Wait for ready outside the device
2. Start the clock
3. Render all the things
4. Get the frame statistics for last frame, make sure that it was presented, otherwise ignore
5. Present the frame, using no wait, (and get the frame id to compare it)
6. Stop the clock and use this number to benchmark
7. Use the time the frame was presented at - the last frame time to do the actual delta calculations
8. Profit
9. Figure out why VSYNC is a thing
10. Use 3 buffers

Measure if doing a lot of work makes it slower?



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