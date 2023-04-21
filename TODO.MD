# TODO
- Terrain Generation is still a bit ugly
    - Can't go back and forth in the panels after creating something
    - we can probably iteratively add erosion, with the building blocks we have, should provide an interesting visual effect

- Grass needs to be tinted based on terrain erosion
- Grass needs to be placed more on flat and less on sloped surfaces
- Terrain needs a tiling texture, see below
- Camera movement is stuttery, even at fixed FPS, is this because the alpha value is not used?
- Without vsync TAA looks really crazy

Use ReadOnlyMemory<> instead of naked arrays!

---

Make sure that changes (adding new textures, creating new entities/components) only happen at a single stage in the pipeline. Just like how deleting only happens on buffer flush. Maybe via some sort of command bus/queue, would also help networking/multiplayer maybe?

---

- Quiting during loading screen doesn't really quit?

- Sun shadows: front faces rendered with slope-scaled depth bias?? https://docs.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-output-merger-stage-depth-bias
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf for shadow map ideas, especially from slide 59
- See https://www.realtimeshadows.com/sites/default/files/Playing%20with%20Real-Time%20Shadows_0.pdf starting form slide 42 on FOG

- Make it easier to change skyboxes
- Use Texture.Load() instead of Sampling using the point sampling for a tiny perf boost where we don't need sampling

- Unsafe zooi: https://github.com/dotnetchrome://vivaldi-webui/startpage?section=Speed-dials&background-color=#2f2f2f/runtime/issues/29905

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
or: https://polycount.com/discussion/116254/terrain-tiling-reduction-multi-uv-mixing

If trying out tasks, there's a lot I can do to figure out where to run tasks. See: https://devblogs.microsoft.com/dotnet/configureawait-faq/ (misleading name) also configureawait(false) seems like a good option to set!