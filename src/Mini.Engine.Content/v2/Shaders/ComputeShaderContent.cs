using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Shaders;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.v2.Shaders;
internal class ComputeShaderContent : IComputeShader, IContent
{
    private IComputeShader original;

    public ComputeShaderContent(ContentId id, IComputeShader original, ComputeShaderSettings settings, ISet<string> dependencies)
    {
        this.Id = id;
        this.Settings = settings;
        this.Dependencies = dependencies;
        
        this.Reload(original);        
    }

    [MemberNotNull(nameof(original))]
    public void Reload(IComputeShader original)
    {
        this.Dispose();
        this.original = original;
    }

    public ContentId Id { get; }    
    public ISet<string> Dependencies { get; }

    public ComputeShaderSettings Settings { get; }

    ID3D11ComputeShader IComputeShader.ID3D11Shader => this.original.ID3D11Shader;

    public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
    {
        return this.original.CreateInputLayout(device, elements);
    }

    public (int X, int Y, int Z) GetDispatchSize(int dimX, int dimY, int dimZ)
    {
        return this.original.GetDispatchSize(dimX, dimY, dimZ);
    }

    public void Dispose()
    {
        this.original?.Dispose();
    }
}
