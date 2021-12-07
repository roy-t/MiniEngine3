using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public class PixelShaderContent : ShaderContent<ID3D11PixelShader>, IPixelShader
{
    public PixelShaderContent(Device device, IVirtualFileSystem fileSystem, string fileName, string entryPoint, string profile)
        : base(device, fileSystem, fileName, entryPoint, profile) { }

    protected override ID3D11PixelShader Create(Blob blob)
    {
        return this.Device.ID3D11Device.CreatePixelShader(blob.GetBytes());
    }
}
