using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Mini.Engine.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public abstract class Shader<TShader> : IContent
        where TShader : ID3D11DeviceChild
    {
        private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

        protected readonly Device Device;

        private Blob blob;

        public Shader(Device device, IVirtualFileSystem fileSystem, string fileName, string entryPoint, string profile)
        {
            this.Device = device;
            this.FileName = fileName;
            this.EntryPoint = entryPoint;
            this.Profile = profile;

            this.Reload(device, fileSystem);
        }

        internal TShader ID3D11Shader { get; set; } = null!;

        public string FileName { get; }
        public string EntryPoint { get; }
        public string Profile { get; }

        [MemberNotNull(nameof(blob))]
        public void Reload(Device device, IVirtualFileSystem fileSystem)
        {
            var sourceText = fileSystem.ReadAllText(this.FileName);
            using var include = new ShaderFileInclude(fileSystem, Path.GetDirectoryName(this.FileName));

            Compiler.Compile(sourceText, Defines, include, this.EntryPoint, this.FileName, this.Profile, out var shaderBlob, out var errorBlob);
            ShaderCompilationErrorFilter.ThrowOnWarningOrError(errorBlob, "X3568");

            this.blob?.Dispose();
            this.ID3D11Shader?.Dispose();

            this.blob = shaderBlob;
            this.ID3D11Shader = this.Create(this.blob);
            this.ID3D11Shader.DebugName = this.FileName;
        }

        protected abstract TShader Create(Blob blob);

        public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
        {
            return new(device.ID3D11Device.CreateInputLayout(elements, this.blob));
        }

        public void Dispose()
        {
            this.blob?.Dispose();
            this.ID3D11Shader?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
