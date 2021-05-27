using System;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public class Shader
    {
        private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

        private ID3D11VertexShader vertexShader;
        private ID3D11PixelShader pixelShader;

        public Shader(ID3D11Device device, string fileName)
            : this(device, fileName, "VS", "vs_5_0", "PS", "ps_5_0") { }

        private Shader(ID3D11Device device,
            string fileName,
            string vertexShaderEntryPoint, string vertexShaderProfile,
            string pixelShaderEntryPoint, string pixelShaderProfile)
        {
            var fullPath = Path.GetFullPath(fileName);
            var sourceText = File.ReadAllText(fullPath);
            var include = new ShaderFileInclude(Path.GetDirectoryName(fullPath));

            Compiler.Compile(sourceText, Defines, include, vertexShaderEntryPoint, fileName, vertexShaderProfile, out var vsBlob, out var vsErrorBlob);
            this.vertexShader = device.CreateVertexShader(vsBlob.GetBytes());

            Compiler.Compile(sourceText, Defines, include, pixelShaderEntryPoint, fileName, pixelShaderProfile, out var psBlob, out var psErrorBlob);
            this.pixelShader = device.CreatePixelShader(psBlob.GetBytes());
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
    }
}
