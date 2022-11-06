using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.v2.Serialization;
public sealed class ContentWriter : IDisposable
{
    public static char DependencySeperator = ';';

    public ContentWriter(Stream stream)
    {
        this.Writer = new BinaryWriter(stream, Encoding.UTF8, true);
    }

    public BinaryWriter Writer { get; }

    public void WriteHeader(Guid header, int version, ISet<string> dependencies)
    {        
        this.WriteType(header);
        this.Writer.Write(version);
        this.WriteDependencies(dependencies);        
    }

    public void Write(ContentId id)
    {
        this.Writer.Write(id.Path);
        this.Writer.Write(id.Key);
    }

    public void WriteArray(ReadOnlySpan<byte> bytes)
    {
        this.Writer.Write(bytes.Length);
        this.Writer.Write(bytes);
    }

    public void Write(TextureLoaderSettings textureSettings)
    {
        this.Writer.Write((int)textureSettings.Mode);
        this.Writer.Write(textureSettings.ShouldMipMap);
    }

    public void Write(MaterialLoaderSettings materialSettings)
    {
        this.Write(materialSettings.AlbedoFormat);
        this.Write(materialSettings.MetalicnessFormat);
        this.Write(materialSettings.NormalFormat);
        this.Write(materialSettings.RoughnessFormat);
        this.Write(materialSettings.AmbientOcclusionFormat);
    }

    public void Write(ModelLoaderSettings modelSettings)
    {
        this.Write(modelSettings.MaterialSettings);
    }

    public void Dispose()
    {
        this.Writer.Flush();
        this.Writer.Dispose();
    }

    private void WriteType(Guid header)
    {
        this.Writer.Write(header.ToByteArray());
        this.Writer.Write(DateTime.Now.Ticks);
    }

    private void WriteDependencies(ISet<string> dependencies)
    {
        var dependencyString = string.Join(DependencySeperator, dependencies);
        this.Writer.Write(dependencyString);
    }
}
