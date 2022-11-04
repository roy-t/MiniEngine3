using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class ComputeShaderProcessor : IContentProcessor<ComputeShaderContent, ComputeShaderSettings>
{
    private static readonly Guid Header = new("{8891B57B-C52C-4933-B121-FE6C718DB3D7}");
    private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();
    private static readonly string Profile = "cs_5_0";

    private readonly Device Device;

    public ComputeShaderProcessor(Device device)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<ComputeShaderContent>();
    }

    public IContentTypeCache<ComputeShaderContent> Cache { get; }
    public int Version => 1;

    public void Generate(ContentId id, ComputeShaderSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        var sourceText = fileSystem.ReadAllText(id.Path);
        using var include = new ShaderFileInclude(fileSystem, Path.GetDirectoryName(id.Path));

        Compiler.Compile(sourceText, Defines, include, id.Key, id.Path, Profile, out var shaderBlob, out var errorBlob);
        ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568" /*Undefined Pragma */);

        writer.WriteHeader(Header, this.Version, fileSystem.GetDependencies());
        writer.Writer.Write(settings.NumThreadsX);
        writer.Writer.Write(settings.NumThreadsY);
        writer.Writer.Write(settings.NumThreadsZ);
        writer.WriteArray(shaderBlob.AsSpan());

        shaderBlob?.Dispose();
        errorBlob?.Dispose();
    }

    public ComputeShaderContent Load(ContentId contentId, ContentHeader header, ContentReader reader)
    {
        var numThreadsX = reader.Reader.ReadInt32();
        var numThreadsY = reader.Reader.ReadInt32();
        var numThreadsZ = reader.Reader.ReadInt32();
        var byteCode = reader.ReadArray();

        var shader = new ComputeShader(this.Device, byteCode, numThreadsX, numThreadsY, numThreadsZ);

        return new ComputeShaderContent(contentId, shader, new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ), header.Dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        var computeShader = (ComputeShaderContent)original;
        this.Generate(original.Id, computeShader.Settings, writerReader.Writer, fileSystem);

        writerReader.Rewind();

        var header = writerReader.Reader.ReadHeader();
        var replacement = this.Load(original.Id, header, writerReader.Reader);
        computeShader.Reload(replacement);
    }
}
