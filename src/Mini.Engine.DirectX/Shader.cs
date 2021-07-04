using System;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public class ShaderBuffer
    {
    }

    public class Shader : IDisposable
    {
        private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

        private readonly Device Device;

        private ID3D11VertexShader vertexShader;
        private Blob vertexShaderBlob;
        private ID3D11PixelShader pixelShader;
        private Blob pixelShaderBlob;

        public Shader(Device device, string fileName)
            : this(device, fileName, "VS", "vs_5_0", "PS", "ps_5_0") { }

        private Shader(Device device,
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
            // Files are read via .NET methods
            var sourceText = File.ReadAllText(this.FullPath);
            using var include = new ShaderFileInclude(Path.GetDirectoryName(this.FullPath));

            Compiler.Compile(sourceText, Defines, include, this.VertexShaderEntryPoint, this.FileName, this.VertexShaderProfile, out this.vertexShaderBlob, out var vsErrorBlob);
            this.vertexShader = this.Device.ID3D11Device.CreateVertexShader(this.vertexShaderBlob.GetBytes());

            Compiler.Compile(sourceText, Defines, include, this.PixelShaderEntryPoint, this.FileName, this.PixelShaderProfile, out this.pixelShaderBlob, out var psErrorBlob);
            this.pixelShader = this.Device.ID3D11Device.CreatePixelShader(this.pixelShaderBlob.GetBytes());
        }

        public void Set(DeviceContext context)
        {
            context.ID3D11DeviceContext.VSSetShader(this.vertexShader);
            context.ID3D11DeviceContext.PSSetShader(this.pixelShader);
            context.ID3D11DeviceContext.GSSetShader(null);
            context.ID3D11DeviceContext.HSSetShader(null);
            context.ID3D11DeviceContext.DSSetShader(null);
            context.ID3D11DeviceContext.CSSetShader(null);
        }

        public InputLayout CreateInputLayout(params InputElementDescription[] elements)
            => new(this.Device.ID3D11Device.CreateInputLayout(elements, this.vertexShaderBlob));

        public void Dispose()
        {
            this.vertexShaderBlob?.Dispose();
            this.vertexShader?.Dispose();
            this.pixelShaderBlob?.Dispose();
            this.pixelShader?.Dispose();
        }
    }
}
