# TODO

- Sun shadows: front faces rendered with slope-scaled depth bias?? https://docs.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-output-merger-stage-depth-bias
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf for shadow map ideas, especially from slide 59
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf starting form slide 42 on FOG

- Only run generators on each compile, some generators now run on each change and look at each method for an attribute!

- Make it easier to change skyboxes
- Use Texture.Load() instead of Sampling using the point sampling for a tiny perf boost where we don't need sampling

- Check RenderTargetViews, ShaderResourceViews of it still makes sense, is there not 1 overload per resource?

Continue replacing things in Mini.Engine.Windows with this: https://github.com/microsoft/CsWin32

# Notes
Particle renderer
https://miketuritzin.com/post/rendering-particles-with-compute-shaders/

Tips on handling raw memory using a shared pool `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();`
https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines

Rendering with command lists could speed things up IFF we adhere to all the ideas in that slideshow.
https://developer.nvidia.com/sites/default/files/akamai/gamedev/docs/GDC_2013_DUDASH_DeferredContexts.pdf)

Due to the new flip mode and tearing support users need at least Windows 10 Anniversary edition (2016)
See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/variable-refresh-rate-displays

Ideas for terain rendering without repeats (stochastic texturing), blog contains other tips about terrains
https://medium.com/@jasonbooth_86226/stochastic-texturing-3c2e58d76a14