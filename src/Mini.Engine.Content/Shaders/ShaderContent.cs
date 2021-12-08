using System;
using System.IO;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public abstract class ShaderContent<TShader> : Shader<TShader>, IContent
    where TShader : ID3D11DeviceChild
{
    private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();
    private readonly IVirtualFileSystem FileSystem;

    public ShaderContent(Device device, IVirtualFileSystem fileSystem, string fileName, string entryPoint, string profile)
        : base(device)
    {
        this.FileSystem = fileSystem;
        this.FileName = fileName;
        this.EntryPoint = entryPoint;
        this.Profile = profile;

        this.Reload(device);
    }

    public string FileName { get; }
    public string EntryPoint { get; }
    public string Profile { get; }

    public void Reload(Device device)
    {
        var sourceText = this.FileSystem.ReadAllText(this.FileName);
        using var include = new ShaderFileInclude(this.FileSystem, Path.GetDirectoryName(this.FileName));

        Compiler.Compile(sourceText, Defines, include, this.EntryPoint, this.FileName, this.Profile, out var shaderBlob, out var errorBlob);
        ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568");

        this.blob?.Dispose();
        this.ID3D11Shader?.Dispose();

        this.blob = shaderBlob;
        this.ID3D11Shader = this.Create(this.blob);
        this.ID3D11Shader.DebugName = this.FileName;
    }

    protected abstract TShader Create(Blob blob);
}