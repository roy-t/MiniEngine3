﻿using LibGame.Graphics;
using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;
public sealed class ZoneTerrainColorizer : ITerrainColorizer
{
    private readonly ZoneOptimizer.State State;
    private readonly ColorLinear[] AllColors;
    public ZoneTerrainColorizer(Tile[] tiles, int columns, int rows)
    {
        this.State = ZoneOptimizer.Optimize(tiles, columns, rows);

        this.AllColors = new ColorLinear[256 * 256 * 256];

        // Horribly in efficient way to do this if we already know how many tiles we have :P

        var i = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    this.AllColors[i++] = new ColorLinear(r / 255.0f, g / 255.0f, b / 255.0f);
                }
            }
        }
    }

    public ColorLinear GetColor(IReadOnlyList<Tile> tiles, int i, IReadOnlyList<TerrainVertex> vertices, int a, int b, int c)
    {
        var owner = this.State.Owners[i];
        var index = (int)Ranges.Map(owner, (0.0f, this.State.Zone), (0.0f, (256.0f * 256.0f * 256.0f) - 1.0f));
        return this.AllColors[index];
    }
}
