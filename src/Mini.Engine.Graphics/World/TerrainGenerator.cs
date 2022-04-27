﻿using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Serilog;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainGenerator
{
    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly HeightMapGenerator HeightMapGenerator;
    private readonly ErosionBrush ErosionBrush;
    private readonly ContentManager Content;

    private static readonly Color4 Umber = new Color4(140.0f / 255.0f, 105.0f / 255.0f, 75.0f / 255.0f);

    public TerrainGenerator(ILogger logger, Device device, ContentManager content, HeightMapGenerator noiseGenerator, ErosionBrush erosionBrush)
    {
        this.Logger = logger.ForContext<TerrainGenerator>();
        this.Device = device;
        this.HeightMapGenerator = noiseGenerator;
        this.Content = content;
        this.ErosionBrush = erosionBrush;
    }
    // TODO: double check which resources should be tied to the content manager and/or should be disposed

    public TerrainComponent Generate(Entity entity, int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, string name)
    {
        var stopwatch = Stopwatch.StartNew();
        var height = this.HeightMapGenerator.GenerateHeights(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance, entity);
        var normals = this.HeightMapGenerator.GenerateNormals(height, entity);
        var tint = this.HeightMapGenerator.GenerateTint(dimensions, Umber, entity);

        this.Logger.Information("Terrain generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        var bounds = ComputeBounds(amplitude, octaves, persistance);
        var mesh = this.GenerateMesh(height, normals, bounds, name);        
        

        return new TerrainComponent(entity, height, normals, tint, mesh);
    }

    public TerrainComponent Erode(Entity world, TerrainComponent terrain, int droplets, string name)
    {
        var height = (RWTexture2D)terrain.Height;
        var tint = (RWTexture2D)terrain.Tint;

        this.ErosionBrush.Apply(height, tint, droplets);

        var normals = this.HeightMapGenerator.GenerateNormals(height, world);
        var mesh = this.GenerateMesh(height, normals, terrain.Mesh.Bounds, name);

        return new TerrainComponent(world, height, normals, terrain.Tint, mesh);
    }

    private IMesh GenerateMesh(RWTexture2D height, RWTexture2D normals, BoundingBox bounds, string name)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height, normals);
        var indices = this.HeightMapGenerator.GenerateIndices(height.Width, height.Height);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, name, "mesh");
        this.Content.Link(mesh, $"{name}#terrain");

        return mesh;
    }

    private static BoundingBox ComputeBounds(float amplitude, int octaves, float persistance)
    {
        var weight = 0.0f;
        for (var i = 0; i < octaves; i++)
        {
            weight += MathF.Pow(Math.Abs(persistance), i) * amplitude;
        }

        var min = new Vector3(-0.5f, -0.5f * weight, -0.5f);
        var max = new Vector3(0.5f, 0.5f * weight, 0.5f);
        return new BoundingBox(min, max);
    }
}