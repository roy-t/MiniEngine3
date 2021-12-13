# TODO

Content
- Reloading a model reduces memory usage ???
- ShaderContent is now part of Mini.Engine.Content, but doesn't have a content loader (or caching) yet
- Check if there's still a way to hide the DirectX Shader type



Transform should not be a struct as its way too big

Continue replacing things in Mini.Engine.Windows with this: https://github.com/microsoft/CsWin32


# Notes
Particle renderer
https://miketuritzin.com/post/rendering-particles-with-compute-shaders/

Tips on handling raw memory using a shared pool `IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent();
https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines

Rendering with command lists could speed things up IFF we adhere to all the ideas in that slideshow.
https://developer.nvidia.com/sites/default/files/akamai/gamedev/docs/GDC_2013_DUDASH_DeferredContexts.pdf)

Due to the new flip mode and tearing support users need at least Windows 10 Anniversary edition (2016)
See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/variable-refresh-rate-displays

-------------------------------

Content Loading

Content Types
- Raw
	- Texture -> Single file identified by path OR Packed into file with other textures
	- Sound -> Single file identified by path
	- Geometry -> Single file identified by path
- Composite
	- Model -> Single file identified by path, references other files (materials)
	- Material -> Part of material library file identified by path and id, references other files


IContentDataLoader<T>
	- Load(string id?)



Definitions
- ContentData -> data structure that holds content of file in a uniform way
- Content -> data structure with Buffers or references to other content

- ContentDataLoader<T> -> Load(contentId)
- ContentLoader<T> -> Load(contentId)