using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Vortice.DXGI;
using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.Vegetation;

[Service]
public sealed class GrassGenerator
{
    private static readonly Palette GrassPalette = Palette.GrassWater;
    private readonly Device Device;

    public GrassGenerator(Device device)
    {
        this.Device = device;
    }

    public ILifetime<StructuredBuffer<GrassInstanceData>> GenerateClumpedInstanceData(ref TerrainComponent terrainComponent, in TransformComponent terrainTransform, out int instances)
    {
        var random = new Random(12345);

        var columns = 50;
        var rows = 50;

        var min = -0.5f;
        var max = 0.5f;

        var cellSize = new Vector2((max - min) / columns, (max - min) / rows);
        var maxCellDistance = Math.Max(cellSize.X, cellSize.Y);

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
            var rotation = random.NextSingle() * MathF.PI * 2;
            var clump = GrassClump.Default(position, GrassPalette.Pick(), rotation, random.InRange(0.75f, 1.75f));
            clump.ApplyTint = (c, b, d) => ColorMath.Interpolate(c, b, d / maxCellDistance);
            clump.ApplyScale = (c, b, d) => c + random.InRange(-0.2f, 0.2f);
            clump.ApplyPosition = (c, b, d) => Vector2.Lerp(c, b, Math.Min(1.0f, d / maxCellDistance * 1.75f));
            clump.ApplyRotation = (c, b, d) => Radians.Lerp(c, b, d / maxCellDistance);
            return clump;
        });

        var heightResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Height);
        var height = this.Device.ImmediateContext.GetSurfaceData<float>(heightResource);

        var erosionResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Erosion);
        var erosion = this.Device.ImmediateContext.GetSurfaceData<Half>(erosionResource);

        var normalsResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Normals);
        var normals = this.Device.ImmediateContext.GetSurfaceData<Vector4>(normalsResource);

        var weights = GenerateWeights(height, erosion, normals);

        var distributor = new ObjectDistributor(new Vector2(min, min), new Vector2(max, max), 300, weights, heightResource.DimX);

        var data = distributor.Distribute(1_000_000, v =>
        {
            var data = DebugGrassPlacer.Single(GrassPalette, Random.Shared);

            var ox = ((max - min) / columns) * random.NextSingle();
            var oy = ((max - min) / rows) * random.NextSingle();

            data.Position = new Vector3(Math.Clamp(v.X + ox, min + 0.001f, max - 0.001f), 0, Math.Clamp(v.Y + oy, min + 0.001f, max - 0.001f));
            return data;
        });
        instances = data.Length;

        var foilage = new Vector4[height.Length];

        for (var i = 0; i < data.Length; i++)
        {
            var blade = data[i];
            var position = new Vector2(blade.Position.X, blade.Position.Z);

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

            var range = max - min;
            var x = (int)(((blade.Position.X - min) / range) * heightResource.DimX);
            var y = (int)(((blade.Position.Z - min) / range) * heightResource.DimY);

            var textureIndex = Indexes.ToOneDimensional(x, y, heightResource.DimY);

            foilage[textureIndex] += new Vector4(blade.Tint, 1.0f);

            blade.Position.Y = height[textureIndex];
            blade.Position = Vector3.Transform(blade.Position, terrainTransform.Current.GetMatrix());
            data[i] = blade;
        }

        for (var i = 0; i < foilage.Length; i++)
        {
            foilage[i] /= Math.Max(foilage[i].W, 1.0f);
        }

        // This looks like a good start, but definitely needs some blurring, or something like that?
        this.CreateFoilageMap(foilage, heightResource.DimX, heightResource.DimY, ref terrainComponent);

        return this.ArrayToResource(data);
    }

    private void CreateFoilageMap(Vector4[] foilage, int dimX, int dimY, ref TerrainComponent component)
    {
        var format = Format.R32G32B32A32_Float;
        var image = new ImageInfo(dimX, dimY, format, dimX * format.BytesPerPixel());
        var texture = new Texture(this.Device, "Foilage", image, MipMapInfo.None());

        texture.SetPixels<Vector4>(this.Device, foilage);

        component.Foilage = this.Device.Resources.Add(texture);
    }

    private ILifetime<StructuredBuffer<GrassInstanceData>> ArrayToResource(GrassInstanceData[] data)
    {
        var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        return this.Device.Resources.Add(instanceBuffer);
    }

    private float[] GenerateWeights(float[] height, Half[] erosion, Vector4[] normals)
    {
        var weights = new float[height.Length];
        for (var i = 0; i < height.Length; i++)
        {
            var w = 0.0f;

            var h = height[i];
            var e = UnpackErosion(erosion[i]);
            var n = UnpackNormal(normals[i]);

            var dot = Vector3.Dot(Vector3.UnitY, n);
            if (dot > 0.975f)
            {
                w = 1.0f;
            }

            if (e < 0.000001f)
            {
                w = 0.0f;
            }

            if (h > 0.01f)
            {
                w = 0.0f;
            }

            weights[i] = w;
        }

        return weights;
    }

    private static Vector3 UnpackNormal(Vector4 normal)
    {
        return new Vector3(normal.X, normal.Y, normal.Z);
    }

    private static float UnpackErosion(Half erosion)
    {
        return (float)erosion;
    }
}
