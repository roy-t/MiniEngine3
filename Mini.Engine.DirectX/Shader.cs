using System;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class Shader : IDisposable
    {
        private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

        private readonly ID3D11Device Device;

        private ID3D11VertexShader vertexShader;
        private Blob vertexShaderBlob;
        private ID3D11PixelShader pixelShader;

        public Shader(ID3D11Device device, string fileName)
            : this(device, fileName, "VS", "vs_5_0", "PS", "ps_5_0") { }

        private Shader(ID3D11Device device,
            string fileName,
            string vertexShaderEntryPoint, string vertexShaderProfile,
            string pixelShaderEntryPoint, string pixelShaderProfile)
        {
            this.Device = device;
            this.FileName = fileName;
            this.FullPath = Path.GetFullPath(fileName);
            this.VertexShaderEntryPoint = vertexShaderEntryPoint;
            this.VertexShaderProfile = vertexShaderProfile;
            this.PixelShaderEntryPoint = pixelShaderEntryPoint;
            this.PixelShaderProfile = pixelShaderProfile;

            this.Reload();
        }

        public string FileName { get; }
        public string FullPath { get; }
        public string VertexShaderEntryPoint { get; }
        public string VertexShaderProfile { get; }
        public string PixelShaderEntryPoint { get; }
        public string PixelShaderProfile { get; }

        public void Reload()
        {
            var sourceText = File.ReadAllText(this.FullPath);
            var include = new ShaderFileInclude(Path.GetDirectoryName(this.FullPath));

            Compiler.Compile(sourceText, Defines, include, this.VertexShaderEntryPoint, this.FileName, this.VertexShaderProfile, out this.vertexShaderBlob, out var vsErrorBlob);
            this.vertexShader = this.Device.CreateVertexShader(this.vertexShaderBlob.GetBytes());

            Compiler.Compile(sourceText, Defines, include, this.PixelShaderEntryPoint, this.FileName, this.PixelShaderProfile, out var psBlob, out var psErrorBlob);
            this.pixelShader = this.Device.CreatePixelShader(psBlob.GetBytes());
        }

        public void Set(ID3D11DeviceContext context)
        {
            context.VSSetShader(this.vertexShader);
            context.PSSetShader(this.pixelShader);
            context.GSSetShader(null);
            context.HSSetShader(null);
            context.DSSetShader(null);
            context.CSSetShader(null);
        }

        public ID3D11InputLayout CreateInputLayout(params InputElementDescription[] elements)
            => this.Device.CreateInputLayout(elements, this.vertexShaderBlob);

        public void Dispose()
        {
            this.vertexShaderBlob?.Release();
            this.vertexShader?.Release();
            this.pixelShader?.Release();
        }
    }
}
