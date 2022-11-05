using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class PixelShaderContent : ShaderContent<IPixelShader, PixelShaderSettings>, IPixelShader
{
    public PixelShaderContent(ContentId id, IPixelShader original, PixelShaderSettings settings, ISet<string> dependencies)
        : base(id, original, settings, dependencies) { }

    ID3D11PixelShader IPixelShader.ID3D11Shader => this.original.ID3D11Shader;
}
