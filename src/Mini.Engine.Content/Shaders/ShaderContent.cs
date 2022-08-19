using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public interface IShaderContent : IContent, IDisposable
{
    string Profile { get; }
}

public abstract class ShaderContent<TShader> : Shader<TShader>, IShaderContent
    where TShader : ID3D11DeviceChild
{
    private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();
    private readonly IVirtualFileSystem FileSystem;
    private readonly ContentManager Content;

    public ShaderContent(Device device, IVirtualFileSystem fileSystem, ContentManager content, ContentId id, string profile)
        : base(device)
    {
        this.FileSystem = fileSystem;
        this.Content = content;
        this.Id = id;
        this.Profile = profile;

        this.Reload(device);

        this.Content.Add(this);
    }

    public ContentId Id { get; }
    public string Profile { get; }

    public void Reload(Device device)
    {
        var sourceText = this.FileSystem.ReadAllText(this.Id.Path);
        using var include = new ShaderFileInclude(this.FileSystem, Path.GetDirectoryName(this.Id.Path));

        Compiler.Compile(sourceText, Defines, include, this.Id.Key, this.Id.Path, this.Profile, out var shaderBlob, out var errorBlob);
        ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568" /*Undefined Pragma */);

        this.blob?.Dispose();
        this.ID3D11Shader?.Dispose();

        this.blob = shaderBlob;
        this.ID3D11Shader = this.Create(this.blob);
        this.ID3D11Shader.DebugName = DebugNameGenerator.GetName(this.Id.ToString(), "SHADER");

        foreach (var pathRelativeToShader in include.Included)
        {
            var pathRelativeToRoot = Path.Combine(Path.GetDirectoryName(this.Id.Path) ?? string.Empty, pathRelativeToShader);
            var pathNormalized = this.FileSystem.NormalizePath(pathRelativeToRoot);
            this.Content.RegisterDependency(this.Id, pathNormalized);
        }
    }

    protected abstract TShader Create(Blob blob);

    public override string ToString()
    {
        return $"Shader: {this.Id}";
    }
}