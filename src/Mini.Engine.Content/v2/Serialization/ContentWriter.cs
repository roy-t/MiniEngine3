using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.v2.Serialization;
public sealed class ContentWriter : IDisposable
{    
    public ContentWriter(Stream stream)
    {
        this.Writer = new BinaryWriter(stream, Encoding.UTF8, true);
    }

    public BinaryWriter Writer { get; }

    public void WriteCommon(Guid header, ContentRecord meta, ISet<string> dependencies, ReadOnlySpan<byte> contents)
    {        
        this.WriteHeader(header);
        this.WriteMeta(meta);
        this.WriteDependencies(dependencies);
        this.WriteContents(contents);
    }

    private void WriteHeader(Guid header)
    {
        this.Writer.Write(header.ToByteArray());
        this.Writer.Write(DateTime.Now.Ticks);
    }

    private void WriteMeta(ContentRecord record)
    {
        this.Write(record.TextureSettings);
        this.Write(record.MaterialSettings);
        this.Write(record.ModelSettings);
    }

    private void WriteDependencies(ISet<string> dependencies)
    {
        var dependencyString = string.Join(Constants.StringSeperator, dependencies);
        this.Writer.Write(dependencyString);
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

    public void Dispose()
    {
        this.Writer.Flush();
        this.Writer.Dispose();
    }
}
