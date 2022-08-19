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

    /// <summary>
    /// Generates 2-dimensional height map and its corresponding normal map using simplex noise
    /// </summary>
    /// <param name="dimensions">Length of each dimension</param>
    /// <param name="amplitude">Amplitude of first octave</param>
    /// <param name="frequency">Frequency of first octave</param>
    /// <param name="octaves">Number of layers of noise</param>
    /// <param name="lacunarity">(1..) Increase in frequency for each consecutive octave, l * f ^ 0, l * f ^1, ...</param>
    /// <param name="persistance">[0..1), Decrease of amplitude for each consecutive octage, p * f ^ 0, p * f ^ 1, ...</param>
    /// <returns></returns>
    public IRWTexture GenerateHeights(HeightMapGeneratorSettings settings)
    {
        var height = new RWTexture(this.Device, nameof(HeightMapGenerator) + "HeightMap", new ImageInfo(settings.Dimensions, settings.Dimensions, Format.R32_Float), MipMapInfo.None());
        this.UpdateHeights(height, settings);

        return height;
    }

    public void UpdateHeights(IRWTexture height, HeightMapGeneratorSettings settings)
    {
        var context = this.Device.ImmediateContext;
        var dimensions = height.DimX;

        this.User.MapNoiseConstants(context, (uint)dimensions, settings.Offset, settings.Amplitude, settings.Frequency, settings.Octaves, settings.Lacunarity, settings.Persistance, settings.CliffStart, settings.CliffEnd, settings.CliffStrength);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        context.CS.SetShader(this.Shader.NoiseMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, height);

        var (x, y, z) = this.Shader.NoiseMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        context.CS.ClearUnorderedAccessView(HeightMap.MapHeight);
    }

    public IRWTexture GenerateNormals(IRWTexture heightMap)
    {
        var dimensions = heightMap.DimX;
        var normals = new RWTexture(this.Device, nameof(HeightMapGenerator) + "NormalMap", new ImageInfo(dimensions, dimensions, Format.R32G32B32A32_Float), MipMapInfo.None());

        this.UpdateNormals(heightMap, normals);

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

        var (x, y, z) = this.Shader.NormalMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);
    }

    public IRWTexture GenerateTint(int dimensions, Color4 tint)
    {
        var texture = new RWTexture(this.Device, nameof(HeightMapGenerator) + "Tint", new ImageInfo(dimensions, dimensions, Format.R32G32B32A32_Float), MipMapInfo.None());
        this.UpdateTint(texture, tint);

        return texture;
    }

    public void UpdateTint(IRWTexture texture, Color4 tint)
    {
        this.Device.ImmediateContext.Clear(texture, tint);
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
        var (x, y, z) = this.Shader.TriangulateKernel.GetDispatchSize(width, height, 1);
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
        var (x, y, z) = this.Shader.IndicesKernel.GetDispatchSize(indices, 1, 1);
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
