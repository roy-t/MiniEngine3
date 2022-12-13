using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
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
    
    public ILifetime<StructuredBuffer<GrassInstanceData>> GenerateClumpedInstanceData(in TerrainComponent terrainComponent, in TransformComponent terrainTransform, out int instances)
    {
        var random = new Random(12345);

        var bladesPerSide = 1000;
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

        // TODO: now that we can place items based on weights/textures, what to do with it?

        var heightResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Height);
        var height = this.Device.ImmediateContext.GetSurfaceData<float>(heightResource);

        var erosionResource = (RWTexture)this.Device.Resources.Get(terrainComponent.Erosion);
        var erosion = this.Device.ImmediateContext.GetSurfaceData<float>(erosionResource);

        //var data = DebugGrassPlacer.GenerateRandomGrass(GrassPalette, bladesPerSide * bladesPerSide, min, max);
        var distributor = new ObjectDistributor(new Vector2(min, min), new Vector2(max, max), 300, height, heightResource.DimX);
        
        var data = distributor.Distribute(1_000_000, v => {
            var data = DebugGrassPlacer.Single(GrassPalette, Random.Shared);
            data.Position = new Vector3(v.X, 0.0f, v.Y);
            return data;            
        });
        instances = data.Length;
        
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


            blade.Position.Y = height[Indexes.ToOneDimensional(x, y, heightResource.DimY)];       
            blade.Position = Vector3.Transform(blade.Position, terrainTransform.Current.GetMatrix());
            data[i] = blade;
        }

        return this.ArrayToResource(data);
    }

    private ILifetime<StructuredBuffer<GrassInstanceData>> ArrayToResource(GrassInstanceData[] data)
    {
        var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        return this.Device.Resources.Add(instanceBuffer);
    }
}
