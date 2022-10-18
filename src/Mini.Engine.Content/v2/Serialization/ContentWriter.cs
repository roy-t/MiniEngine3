using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Serialization;
internal sealed class ContentWriter : IDisposable
{
    private readonly Stream Stream;
    private readonly BinaryWriter Writer;

    public ContentWriter(ContentId id, IVirtualFileSystem fileSystem)
    {
        var path = id.Path + Constants.Extension;

        this.Stream = fileSystem.OpenWrite(path);
        this.Writer = new BinaryWriter(this.Stream);        
    }

    public void WriteAll(Guid header, ContentRecord meta, IReadOnlyList<string> dependencies, ReadOnlySpan<byte> contents)
    {     
        this.WriteHeader(header);
        this.WriteMeta(meta);
        this.WriteDependencies(dependencies);
        this.WriteContents(contents);        
    }

    public void Dispose()
    {
        this.Writer.Flush();
        this.Writer.Dispose();
        this.Stream.Dispose();
    }

    private void WriteHeader(Guid header)
    {
        this.Writer.Write(header.ToByteArray());
    }

    private void WriteMeta(ContentRecord record)
    {
        this.Write(record.TextureSettings);
        this.Write(record.MaterialSettings);
        this.Write(record.ModelSettings);
    }

    private void WriteDependencies(IReadOnlyList<string> dependencies)
    {
        this.Writer.Write(string.Join(Constants.StringSeperator, dependencies));
    }

    private void WriteContents(ReadOnlySpan<byte> bytes)
    {
        this.Writer.Write(bytes.Length);
        this.Writer.Write(bytes);
    }

    private void Write(TextureLoaderSettings textureSettings)
    {
        this.Writer.Write((int)textureSettings.Mode);
        this.Writer.Write(textureSettings.ShouldMipMap);
    }

    private void Write(MaterialLoaderSettings materialSettings)
    {
        this.Write(materialSettings.AlbedoFormat);
        this.Write(materialSettings.MetalicnessFormat);
        this.Write(materialSettings.NormalFormat);
        this.Write(materialSettings.RoughnessFormat);
        this.Write(materialSettings.AmbientOcclusionFormat);
    }

    private void Write(ModelLoaderSettings modelSettings)
    {
        this.Write(modelSettings.MaterialSettings);
    }
}
