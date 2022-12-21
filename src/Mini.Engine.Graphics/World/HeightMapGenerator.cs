using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HeightMapGenerator : IDisposable
{
    private readonly Device Device;
    private readonly HeightMap Shader;
    private readonly HeightMap.User User;

    public HeightMapGenerator(Device device, HeightMap shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<HeightMapGenerator>();
    }

    public IRWTexture GenerateEmtpyHeights(int dimensions)
    {
        return new RWTexture(this.Device, nameof(HeightMapGenerator) + "HeightMap", new ImageInfo(dimensions, dimensions, Format.R32_Float), MipMapInfo.None());
    }

    public void UpdateHeights(IRWTexture height, HeightMapGeneratorSettings settings)
    {
        var context = this.Device.ImmediateContext;
        var dimensions = height.DimX;

        this.User.MapNoiseConstants(context, (uint)dimensions, settings.Offset, settings.Amplitude, settings.Frequency, settings.Octaves, settings.Lacunarity, settings.Persistance, settings.CliffStart, settings.CliffEnd, settings.CliffStrength);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        context.CS.SetShader(this.Shader.NoiseMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, height);

        var (x, y, z) = this.Shader.GetDispatchSizeForNoiseMapKernel(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        context.CS.ClearUnorderedAccessView(HeightMap.MapHeight);
    }

    public IRWTexture GenerateEmptyNormals(int dimensions)
    {
        var normals = new RWTexture(this.Device, nameof(HeightMapGenerator) + "NormalMap", new ImageInfo(dimensions, dimensions, Format.R32G32B32A32_Float), MipMapInfo.None());
        this.Device.ImmediateContext.Clear(normals, new Color4(0, 1, 0, 0));

        return normals;
    }

    public void UpdateNormals(IRWTexture heightMap, IRWTexture normals)
    {
        var context = this.Device.ImmediateContext;
        var dimensions = normals.DimX;

        this.User.MapNoiseConstants(context, (uint)dimensions, Vector2.Zero, 0, 0, 0, 0, 0, 0, 0, 0);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        context.CS.SetShader(this.Shader.NormalMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMap.MapNormal, normals);

        var (x, y, z) = this.Shader.GetDispatchSizeForNormalMapKernel(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);
    }

    public IRWTexture GenerateTint(int dimensions)
    {
        var texture = new RWTexture(this.Device, nameof(HeightMapGenerator) + "Tint", new ImageInfo(dimensions, dimensions, Format.R16_Float), MipMapInfo.None());
        this.UpdateTint(texture);

        return texture;
    }

    public void UpdateTint(IRWTexture texture)
    {
        this.Device.ImmediateContext.Clear(texture, Colors.Transparent);
    }

    public ModelVertex[] GenerateVertices(IRWTexture heightMap, int width, int height)
    {
        var context = this.Device.ImmediateContext;

        this.User.MapTriangulateConstants(context, (uint)width, (uint)height, (uint)heightMap.DimX, (uint)heightMap.DimY, 0, 0);
        context.CS.SetConstantBuffer(HeightMap.TriangulateConstantsSlot, this.User.TriangulateConstantsBuffer);
        var length = width * height;
        using var output = new RWStructuredBuffer<ModelVertex>(this.Device, nameof(HeightMapGenerator), length);

        context.CS.SetShader(this.Shader.TriangulateKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMap.Vertices, output);
        var (x, y, z) = this.Shader.GetDispatchSizeForTriangulateKernel(width, height, 1);
        context.CS.Dispatch(x, y, z);

        var data = new ModelVertex[length];
        output.ReadData(context, data);

        var minX = data.Min(foo => foo.Position.X);
        var maxX = data.Max(foo => foo.Position.X);
        var minY = data.Min(foo => foo.Position.Y);
        var maxY = data.Max(foo => foo.Position.Y);
        var minZ = data.Min(foo => foo.Position.Z);
        var maxZ = data.Max(foo => foo.Position.Z);

        return data;
    }

    public int[] GenerateIndices(int width, int height)
    {
        var context = this.Device.ImmediateContext;

        var intervals = width - 1;
        var quads = intervals * intervals;
        var triangles = quads * 2;
        var indices = triangles * 3;

        this.User.MapTriangulateConstants(context, (uint)width, (uint)height, 0, 0, (uint)indices, (uint)intervals);
        context.CS.SetConstantBuffer(HeightMap.TriangulateConstantsSlot, this.User.TriangulateConstantsBuffer);

        using var output = new RWStructuredBuffer<int>(this.Device, nameof(HeightMapGenerator), indices);

        context.CS.SetShader(this.Shader.IndicesKernel);
        context.CS.SetUnorderedAccessView(HeightMap.Indices, output);
        var (x, y, z) = this.Shader.GetDispatchSizeForIndicesKernel(indices, 1, 1);
        context.CS.Dispatch(x, y, z);

        var data = new int[indices];
        output.ReadData(context, data);

        return data;
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}
