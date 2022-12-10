using System.Numerics;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX;

using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;
using Mini.Engine.Content;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Graphics.Vegetation;

[Service]
public sealed class DebugGrassPlacer
{
    public enum DebugGrassLayout
    {
        Single,
        Line,
        Random
    }

    private readonly Device Device;
    private readonly ContentManager Content;

    public DebugGrassPlacer(Device device, ContentManager content)
    {
        this.Device = device;
        this.Content = content;
    }

    public ILifetime<StructuredBuffer<GrassInstanceData>> GenerateDebugGrass(DebugGrassLayout layout, Palette palette, out int instances)
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
                data = GenerateRandomGrass(palette, instances);
                break;
        }

        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        return this.Device.Resources.Add(instanceBuffer);
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
        var step = MathF.PI * 2 / (count - 1);
        for (var i = 0; i < data.Length; i++)
        {

            data[i] = new GrassInstanceData()
            {
                Position = new Vector3(minPosition + interval * i, 0.0f, 0.0f),
                Rotation = minRotation + step * i,
                Scale = 1.0f,
                Tint = new Vector3(0.0f, 1.0f, 0.0f)
            };
        }

        return data;
    }

    public static GrassInstanceData[] GenerateRandomGrass(Palette palette, int count, float min = -50, float max = 50)
    {
        var random = new Random(1234);
        var mins = 0.25f;
        var maxs = 1.0f;
        var data = new GrassInstanceData[count];

        for (var i = 0; i < data.Length; i++)
        {
            var x = min + random.NextSingle() * (max - min);
            var y = min + random.NextSingle() * (max - min);
            var s = mins + random.NextSingle() * (maxs - mins);
            var r = random.NextSingle() * MathF.PI * 2;

            var position = new Vector3(x, 0, y);
            var scale = s;
            var rotation = r;

            data[i] = new GrassInstanceData()
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                Tint = palette.Pick()
            };
        }

        return data;
    }
}
