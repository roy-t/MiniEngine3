using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace Mini.Engine.Content.v2.Shaders;
internal abstract class ShaderProcessor<TContent, TWrapped, TSettings> : UnmanagedContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class, IShader, IDisposable
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

    protected readonly Device Device;

    public ShaderProcessor(Device device, Guid typeHeader, int version)
        : base(version, typeHeader, ".hlsl")
    {
        this.Device = device;      
    }    

    public abstract string Profile { get; }

    protected override void WriteBody(ContentId id, TSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var sourceText = fileSystem.ReadAllText(id.Path);
        using var include = new ShaderFileInclude(fileSystem, Path.GetDirectoryName(id.Path));

        Compiler.Compile(sourceText, Defines, include, id.Key, id.Path, this.Profile, out var shaderBlob, out var errorBlob);
        ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568" /*Undefined Pragma */);
      
        writer.WriteArray(shaderBlob.AsSpan());

        shaderBlob?.Dispose();
        errorBlob?.Dispose();
    }

    protected override TContent ReadBody(ContentId id, TSettings settings, ContentReader reader)
    {
        var byteCode = reader.ReadArray();
        return this.Load(id, settings, byteCode);
    }    

    protected abstract TContent Load(ContentId contentId, TSettings settings, byte[] byteCode);        
}
