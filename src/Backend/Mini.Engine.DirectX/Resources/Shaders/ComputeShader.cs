using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Shaders;

public interface IComputeShader : IShader
{
    internal ID3D11ComputeShader ID3D11Shader { get; }
    (int X, int Y, int Z) GetDispatchSize(int dimX, int dimY, int dimZ);
}

public sealed class ComputeShader : IComputeShader
{
    private readonly int NumThreadsX;
    private readonly int NumThreadsY;
    private readonly int NumThreadsZ;

    private readonly byte[] ByteCode;
    private readonly ID3D11ComputeShader Shader;

    public ComputeShader(Device device, byte[] byteCode, int numThreadsX, int numThreadsY, int numThreadsZ)
    {
        this.Shader = device.ID3D11Device.CreateComputeShader(byteCode);
        this.ByteCode = byteCode;

        this.NumThreadsX = numThreadsX;
        this.NumThreadsY = numThreadsY;
        this.NumThreadsZ = numThreadsZ;                
    }

    ID3D11ComputeShader IComputeShader.ID3D11Shader => this.Shader;

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

    public void Dispose()
    {
        this.Shader.Dispose();
    }
}
