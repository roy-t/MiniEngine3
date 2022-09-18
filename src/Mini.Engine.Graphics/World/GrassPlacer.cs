using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Transforms;
using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class GrassPlacer
{
    private readonly Device Device;
    private readonly ContentManager Content;

    public GrassPlacer(Device device, ContentManager content)
    {
        this.Device = device;
        this.Content = content;
    }

    public IResource<StructuredBuffer<GrassInstanceData>> GenerateInstanceData(ref TerrainComponent terrainComponent, ref TransformComponent terrainTransform, out int instances)
    {
        var random = new Random(1234);

        var heightResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Height);
        var normalsResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Height);

        var height = new float[heightResource.ImageInfo.Pixels];
        var normals = new Vector4[normalsResource.ImageInfo.Pixels];

        this.Device.ImmediateContext.CopySurfaceDataToTexture<float>(heightResource, height);
        this.Device.ImmediateContext.CopySurfaceDataToTexture<Vector4>(normalsResource, normals);

        var data = new GrassInstanceData[heightResource.ImageInfo.Pixels];

        var mins = 0.5f;
        var maxs = 1.0f;
        var minColor = new Vector3(100 / 255.0f, 120 / 255.0f, 25.0f / 255.0f);
        var maxColor = new Vector3(140 / 255.0f, 170 / 255.0f, 50.0f / 255.0f);

        for (var y = 0; y < heightResource.DimY; y++)
        {
            for (var x = 0; x < heightResource.DimX; x++)
            {
                var index = Indexes.ToOneDimensional(x, y, heightResource.DimY);

                var s = mins + (random.NextSingle() * (maxs - mins));
                var r = random.NextSingle() * MathF.PI * 2;
                var l = random.NextSingle();

                var h = height[index];
               
                var px = ((x / (float)heightResource.DimX) - 0.5f);
                var pz = ((y / (float)heightResource.DimY) - 0.5f);

                var position = new Vector3(px, h, pz);
                position = Vector3.Transform(position, terrainTransform.Transform.GetMatrix());

                var scale = s;
                var rotation = r;

                data[index] = new GrassInstanceData()
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = scale,
                    Tint = Vector3.Lerp(minColor, maxColor, l)
                };
            }
        }

        var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        var resource = this.Device.Resources.Add(instanceBuffer);
        this.Content.Link(resource, "Grass");

        instances = data.Length;
        return resource;
    }

    // DEBUG stuff

    public enum DebugGrassLayout
    {
        Single,
        Line,
        Random
    }

    public IResource<StructuredBuffer<GrassInstanceData>> GenerateDebugGrass(DebugGrassLayout layout, out int instances)
    {

        var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
        GrassInstanceData[] data;
        switch (layout)
        {
            case DebugGrassLayout.Single:
                instances = 1;
                data = GenerateSingleGrassLeaf();
                break;
            case DebugGrassLayout.Line:
                instances = 8;
                data = GenerateLineOfRotatedGrassLeafs(instances);
                break;
            default:
            case DebugGrassLayout.Random:
                instances = 1_000_000;
                data = GenerateRandomGrass(instances);
                break;
        }

        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        var resource = this.Device.Resources.Add(instanceBuffer);
        this.Content.Link(resource, "Grass");

        return resource;
    }

    private static GrassInstanceData[] GenerateSingleGrassLeaf()
    {
        return new GrassInstanceData[]
        {
            new GrassInstanceData()
            {
                Position = Vector3.Zero,
                Rotation = 0.0f,
                Scale = 1.0f,
                Tint = new Vector3(0.0f, 1.0f, 0.0f)
            }
        };
    }

    private static GrassInstanceData[] GenerateLineOfRotatedGrassLeafs(int count)
    {
        var data = new GrassInstanceData[count];

        var interval = 0.1f;
        var minPosition = count / 2.0f * interval * -1.0f;
        var minRotation = -MathF.PI;
        var step = (MathF.PI * 2) / (count - 1);
        for (var i = 0; i < data.Length; i++)
        {

            data[i] = new GrassInstanceData()
            {
                Position = new Vector3(minPosition + (interval * i), 0.0f, 0.0f),
                Rotation = minRotation + (step * i),
                Scale = 1.0f,
                Tint = new Vector3(0.0f, 1.0f, 0.0f)
            };
        }

        return data;
    }

    private static GrassInstanceData[] GenerateRandomGrass(int count)
    {
        var random = new Random(1234);
        var min = -50.0f;
        var max = 50.0f;
        var mins = 0.5f;
        var maxs = 1.0f;
        var data = new GrassInstanceData[count];

        var minColor = new Vector3(100 / 255.0f, 120 / 255.0f, 25.0f / 255.0f);
        var maxColor = new Vector3(140 / 255.0f, 170 / 255.0f, 50.0f / 255.0f);

        for (var i = 0; i < data.Length; i++)
        {
            var x = min + (random.NextSingle() * (max - min));
            var y = min + (random.NextSingle() * (max - min));
            var s = mins + (random.NextSingle() * (maxs - mins));
            var r = random.NextSingle() * MathF.PI * 2;
            var l = random.NextSingle();

            var position = new Vector3(x, 0, y);
            var scale = s;
            var rotation = r;

            data[i] = new GrassInstanceData()
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                Tint = Vector3.Lerp(minColor, maxColor, l)
            };
        }

        return data;
    }
}
