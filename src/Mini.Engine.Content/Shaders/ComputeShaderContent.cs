using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Content.Shaders;

public class ComputeShaderContent : ShaderContent<ID3D11ComputeShader>, IComputeShader
{
    public ComputeShaderContent(Device device, IVirtualFileSystem fileSystem, ContentManager content, ContentId id, string profile, int numThreadsX, int numThreadsY, int numThreadsZ)
        : base(device, fileSystem, content, id, profile)
    {
        this.NumThreadsX = numThreadsX;
        this.NumThreadsY = numThreadsY;
        this.NumThreadsZ = numThreadsZ;
    }

    public int NumThreadsX { get; }
    public int NumThreadsY { get; }
    public int NumThreadsZ { get; }

    public (int X, int Y, int Z) GetDispatchSize(int dimX, int dimY, int dimZ)
    {
        var x = GetDispatchSize(this.NumThreadsX, dimX);
        var y = GetDispatchSize(this.NumThreadsY, dimY);
        var z = GetDispatchSize(this.NumThreadsZ, dimZ);

        return new(x, y, z);
    }

    private static int GetDispatchSize(int numThreads, int dim)
    {
        return (dim + numThreads - 1) / numThreads;
    }

    protected override ID3D11ComputeShader Create(Blob blob)
    {
        return this.Device.ID3D11Device.CreateComputeShader(blob.GetBytes());
    }
}
