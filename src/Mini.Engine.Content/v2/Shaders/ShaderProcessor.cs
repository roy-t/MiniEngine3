using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace Mini.Engine.Content.v2.Shaders;
internal abstract class ShaderProcessor<TContent, TWrapped, TSettings> : IContentProcessor<TContent, TWrapped, TSettings>
    where TContent : IShader, IDisposable
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

    protected readonly Device Device;

    public ShaderProcessor(Device device, Guid typeHeader, int version)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<TContent>();

        this.TypeHeader = typeHeader;
        this.Version = version;
    }

    public IContentTypeCache<TContent> Cache { get; }
    public Guid TypeHeader {get;}
    public int Version { get; }

    public abstract string Profile { get; }

    public void Generate(ContentId id, TSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        var sourceText = fileSystem.ReadAllText(id.Path);
        using var include = new ShaderFileInclude(fileSystem, Path.GetDirectoryName(id.Path));

        Compiler.Compile(sourceText, Defines, include, id.Key, id.Path, this.Profile, out var shaderBlob, out var errorBlob);
        ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568" /*Undefined Pragma */);

        writer.WriteHeader(this.TypeHeader, this.Version, fileSystem.GetDependencies());
        this.WriteSettings(writer, settings);        
        writer.WriteArray(shaderBlob.AsSpan());

        shaderBlob?.Dispose();
        errorBlob?.Dispose();
    }

    protected abstract void WriteSettings(ContentWriter writer, TSettings settings);

    public TContent Load(ContentId contentId, ContentHeader header, ContentReader reader)
    {
        ContentProcessor.ValidateHeader(this.TypeHeader, this.Version, header);
        var settings = this.LoadSetings(reader);        
        var byteCode = reader.ReadArray();

        return this.Load(contentId, settings, byteCode);        
    }

    protected abstract TSettings LoadSetings(ContentReader reader);

    protected abstract TContent Load(ContentId contentId, TSettings settings, byte[] byteCode);

    public abstract TWrapped Wrap(ContentId id, TContent content, TSettings settings, ISet<string> dependencies);

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (TWrapped)original, fileSystem, writerReader);
    }    
}
