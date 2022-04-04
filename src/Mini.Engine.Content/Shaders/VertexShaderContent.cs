using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public class VertexShaderContent : ShaderContent<ID3D11VertexShader>, IVertexShader
{
    public VertexShaderContent(Device device, IVirtualFileSystem fileSystem, ContentManager content, ContentId id, string profile)
        : base(device, fileSystem, content, id, profile) { }

    protected override ID3D11VertexShader Create(Blob blob)
    {
        return this.Device.ID3D11Device.CreateVertexShader(blob);
    }
}
