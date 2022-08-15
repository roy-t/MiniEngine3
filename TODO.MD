# TODO

-> Before below: first make sure that DeviceResources are neater, especially textures/depthstensils/rendertargets
	-> Always an array
	-> Different per type, not all just only ITexture
	-> Special case for CubeMap?

WHAT IF: all really device resource are just an ID and the texture and shit are just a struct with a reference

---

Make sure that changes (adding new textures, creating new entities/components) only happen at a single stage in the pipeline. Just like how deleting only happens on buffer flush. Maybe via some sort of command bus/queue, would also help networking/multiplayer maybe?

---

Use `typeperf "\GPU Process Memory(*)\Dedicated Usage" -f CSV -o ..\counters4.csv` to print out memory usage, this is a WMI counter that is not available on every system, there are more WMI coutners related to this process. Maybe integrate that in the debug stuff?

---

- Quiting during loading screen doesn't really quit?

- Sun shadows: front faces rendered with slope-scaled depth bias?? https://docs.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-output-merger-stage-depth-bias
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf for shadow map ideas, especially from slide 59
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf starting form slide 42 on FOG

- Make it easier to change skyboxes
- Use Texture.Load() instead of Sampling using the point sampling for a tiny perf boost where we don't need sampling

- Check RenderTargetViews, ShaderResourceViews of it still makes sense, is there not 1 overload per resource?

- Unsafe zooi: https://github.com/dotnet/runtime/issues/29905

- Input: move to https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getrawinputbuffer, see also https://twitter.com/roytries/status/1504726298136043542?s=20&t=f-MV-lcabEsZvyDSDONvMQ


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