using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;
public sealed class ContentManager
{

    public readonly IVirtualFileSystem FileSystem;

    public ContentLoadTask<ITexture> LoadTexture(string path, string key, TextureLoaderSettings settings)
    {
        // ARGGGGGGGGGGGGGGGGG
        // What if instead we just add a ContenLoader to every TextureContent, etc... class
        // that just waits for the data to load asynchronously before replacing it
        // and a blocking method Upload to force it to wait?


        var id = new ContentId(path, key);
        var fileSystem = new TrackingVirtualFileSystem(this.FileSystem);
        var record = new ContentRecord(fileSystem, settings);

        var textureLoader = new v2.Textures.TextureLoader();
        var bytes = textureLoader.Generate();


        // 1. Queue ContentData loading on ThreadPool
        // 2. Wait for ContentData to become available
        // 3. Put Loading Content on Primary Thread Queue
        // 4. Let LoadingScreen push content loading forward?
    }

    public IResource<T> CompleteLoadTask<T>(ContentLoadTask<T> task)
        where T : IDeviceResource
    {

    }
}
