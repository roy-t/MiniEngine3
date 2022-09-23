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

    //public IResource<StructuredBuffer<GrassInstanceData>> GenerateClumpedInstanceData(ref TerrainComponent terrainComponent, ref TransformComponent terrainTransform, out int instances)
    public IResource<StructuredBuffer<GrassInstanceData>> GenerateClumpedInstanceData(out int instances)
    {
        var random = new Random(12345);
        var palette = Palette.Grass();

        var bladesPerSide = 1000;
        var columns = 50;
        var rows = 50;

        var min = -0.5f;
        var max = 0.5f;

        // cellSize * 10 is wrong, but gives a more distorted clumping.. so ehm?
        var cellSize = new Vector2((max - min) / columns, (max - min) / rows) * 1;// * 10;

        var neighbours = new List<Vector2>(9);
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var xOffset = cellSize.X * x;
                var yOffset = cellSize.Y * y;

                neighbours.Add(new Vector2(xOffset, yOffset));
            }
        }

        var clumpGrid = new Grid<GrassClump>(min, max, min, max, columns, rows);
        clumpGrid.Fill((x, y, c, r) =>
        {
            var xOffset = (random.NextSingle() - 0.49f) * cellSize.X;
            var yOffset = (random.NextSingle() - 0.49f) * cellSize.Y;
            
            var position = new Vector2(x + xOffset, y + yOffset);
            var tint = c % 2 == 0? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);
            var clump = GrassClump.Default(position, palette.Pick(), 0.0f, 1.0f);
            //clump.ApplyTint = (c, b, d) => c;
            return clump;
        });

        var data = GenerateRandomGrass(bladesPerSide * bladesPerSide, min, max);
        instances = data.Length;

        for (var i = 0; i < data.Length; i++)
        {
            var blade = data[i];
            var position = new Vector2(blade.Position.X, blade.Position.Z);
            var height = blade.Position.Y;

            var bestClumpDistance = float.MaxValue;
            var bestClump = GrassClump.Default(position, blade.Tint, blade.Rotation, 1.0f);
            for (var n = 0; n < neighbours.Count; n++)
            {
                var absolute = position + neighbours[n];
                var clump = clumpGrid.Get(absolute.X, absolute.Y);

                var distance = Vector2.DistanceSquared(position, clump.Position);
                if (distance < bestClumpDistance)
                {
                    bestClumpDistance = distance;
                    bestClump = clump;
                }
            }

            bestClump.Apply(ref blade);

            //var newPosition = Vector2.Lerp(position, bestClump!.Position, 0.0f);
            //var newTint = bestClump.Tint;

            //blade.Position = new Vector3(position.X, blade.Position.Y, position.Y);
            //blade.Tint = newTint;

            //if(bestClumpDistance < 0.00003f)
            //{
            //    blade.Tint = new Vector3(1, 1, 0);
            //}

            var transform = Transform.Identity.SetScale(100.0f);
            //blade.Position = Vector3.Transform(blade.Position, terrainTransform.Transform.GetMatrix());
            // TODO: set height here from heightmap!
            blade.Position = Vector3.Transform(blade.Position, transform.GetMatrix());
            data[i] = blade;
        }

        return this.ArrayToResource(data);
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

        instances = data.Length;
        return this.ArrayToResource(data);
    }

    private IResource<StructuredBuffer<GrassInstanceData>> ArrayToResource(GrassInstanceData[] data)
    {
        var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        var resource = this.Device.Resources.Add(instanceBuffer);
        this.Content.Link(resource, "Grass");

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

    private static GrassInstanceData[] GenerateRandomGrass(int count, float min = -50, float max = 50)
    {
        var random = new Random(1234);
        var mins = 0.5f;
        var maxs = 1.0f;
        var data = new GrassInstanceData[count];

        //var minColor = new Vector3(100 / 255.0f, 120 / 255.0f, 25.0f / 255.0f);
        var minColor = new Vector3(1.0f, 0.0f, 0.0f);
        //var maxColor = new Vector3(140 / 255.0f, 170 / 255.0f, 50.0f / 255.0f);
        var maxColor = new Vector3(0.0f, 1.0f, 0.0f);


        var h0 = ColorMath.RGBToHSV(minColor);
        var h1 = ColorMath.HSVToRGB(h0);

        for (var i = 0; i < data.Length; i++)
        {
            var x = min + (random.NextSingle() * (max - min));
            var y = min + (random.NextSingle() * (max - min));
            var s = mins + (random.NextSingle() * (maxs - mins));
            var r = random.NextSingle() * MathF.PI * 2;
            var l = x + 0.5f;// random.NextSingle();

            var position = new Vector3(x, 0, y);
            var scale = s;
            var rotation = r;

            data[i] = new GrassInstanceData()
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                Tint = ColorMath.Interpolate(minColor, maxColor, l) //Vector3.Lerp(minColor, maxColor, l)
            };
        }

        return data;
    }
}
