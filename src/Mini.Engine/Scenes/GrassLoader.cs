using System.Numerics;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Vegetation;
using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Scenes;
internal static class GrassLoader
{
    public static LoadAction LoadGrass(Device device, ContentManager content, ECSAdministrator administrator)
    {
        var creator = administrator.Components;

        return new LoadAction("Grass", () =>
        {
            var grass = administrator.Entities.Create();
            ref var grassy = ref creator.Create<GrassComponent>(grass);

            var instanceBuffer = new StructuredBuffer<GrassInstanceData>(device, "Grass");

            var instances = 1000 * 1000;
            var data = GenerateGrass(instances);

            //var instances = 11;
            //var data = GenerateDebugGrass(instances);

            //var instances = 1;
            //var data = GenerateSingleDebugGrass();

            instanceBuffer.MapData(device.ImmediateContext, data);

            var resource = device.Resources.Add(instanceBuffer);
            content.Link(resource, "Grass");
            grassy.InstanceBuffer = resource;
            grassy.Instances = instances;
        });
    }

    private static GrassInstanceData[] GenerateSingleDebugGrass()
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

    private static GrassInstanceData[] GenerateDebugGrass(int count)
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
                Tint = new Vector3(50 / 255.0f, 50 / 255.0f, 10.0f / 255.0f)
            };
        }

        return data;
    }

    private static GrassInstanceData[] GenerateGrass(int count)
    {
        var random = new Random(1234);
        var min = -50.0f;
        var max = 50.0f;
        var mins = 0.5f;
        var maxs = 1.0f;
        var data = new GrassInstanceData[count];

        var minColor = new Vector3(20 / 255.0f, 80 / 255.0f, 20.0f / 255.0f);
        var maxColor = new Vector3(100 / 255.0f, 180 / 255.0f, 70.0f / 255.0f);

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
