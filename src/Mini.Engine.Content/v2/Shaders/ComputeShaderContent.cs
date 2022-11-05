using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.v2.Shaders;

internal sealed class ComputeShaderContent : ShaderContent<IComputeShader, ComputeShaderSettings>, IComputeShader
{
    public ComputeShaderContent(ContentId id, IComputeShader original, ComputeShaderSettings settings, ISet<string> dependencies)
        : base(id, original, settings, dependencies) { }

    ID3D11ComputeShader IComputeShader.ID3D11Shader => this.original.ID3D11Shader;

    public (int X, int Y, int Z) GetDispatchSize(int dimX, int dimY, int dimZ)
    {
        return this.original.GetDispatchSize(dimX, dimY, dimZ);
    }
}
