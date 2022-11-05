using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class VertexShaderContent : ShaderContent<IVertexShader, VertexShaderSettings>, IVertexShader
{
    public VertexShaderContent(ContentId id, IVertexShader original, VertexShaderSettings settings, ISet<string> dependencies)
        : base(id, original, settings, dependencies) { }

    ID3D11VertexShader IVertexShader.ID3D11Shader => this.original.ID3D11Shader;

    public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
    {
        return this.original.CreateInputLayout(device, elements);
    }
}
