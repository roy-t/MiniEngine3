using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.IO;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public class VertexShaderContent : ShaderContent<ID3D11VertexShader>, IVertexShader
{
    public VertexShaderContent(Device device, IVirtualFileSystem fileSystem, ContentManager content, ContentId id, string profile)
        : base(device, fileSystem, content, id, profile) { }

    public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
    {
        return new(device.ID3D11Device.CreateInputLayout(elements, this.blob!));
    }

    protected override ID3D11VertexShader Create(Blob blob)
    {
        return this.Device.ID3D11Device.CreateVertexShader(blob);
    }
}
