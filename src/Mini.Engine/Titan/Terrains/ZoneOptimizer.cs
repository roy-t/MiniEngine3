using LibGame.Mathematics;

namespace Mini.Engine.Titan.Terrains;

public readonly record struct Zone(int StartColumn, int EndColumn, int StartRow, int EndRow);

public sealed record class ZoneLookup(IReadOnlyList<int> Owners, IReadOnlyList<Zone> Zones);

public sealed class ZoneOptimizer
{
    private readonly int[] Owners;

    public ZoneOptimizer(int columns, int rows)
    {
        this.Owners = new int[columns * rows];
        this.Zones = new List<Zone>();
    }

    public List<Zone> Zones { get; }

    public void Clear()
    {
        Array.Clear(this.Owners);
        this.Zones.Clear();
    }

    public ZoneLookup Optimize(IReadOnlyGrid<Tile> tiles)
    {
        var columns = tiles.Columns;
        var rows = tiles.Rows;
        for (var row = 0; row < rows; row++)
        {
            var zone = new Zone(0, 0, row, row);
            while (zone.EndColumn < columns)
            {
                if (CanExpandEast(tiles, in zone, columns))
                {
                    zone = new Zone(zone.StartColumn, zone.EndColumn + 1, row, row);
                }
                else
                {
                    var owner = this.Zones.Count;
                    if (row > 0)
                    {
                        var northernZoneIndex = this.Owners[Indexes.ToOneDimensional(zone.StartColumn, row - 1, columns)];
                        var northernZone = this.Zones[northernZoneIndex];
                        if (northernZone.StartColumn == zone.StartColumn && northernZone.EndColumn == zone.EndColumn &&
                            CanExpandSouth(tiles, in northernZone, columns, rows))
                        {
                            owner = northernZoneIndex;
                            this.Zones[northernZoneIndex] = new Zone(zone.StartColumn, zone.EndColumn, northernZone.StartRow, row);
                        }
                    }

                    for (var c = zone.StartColumn; c <= zone.EndColumn; c++)
                    {
                        this.Owners[Indexes.ToOneDimensional(c, row, columns)] = owner;
                    }
                    if (owner == this.Zones.Count)
                    {
                        this.Zones.Add(zone);
                    }
                    zone = new Zone(zone.EndColumn + 1, zone.EndColumn + 1, row, row);
                }
            }
        }

        return new ZoneLookup(this.Owners, this.Zones);
    }

    private static bool CanExpandEast(IReadOnlyGrid<Tile> tiles, in Zone zone, int columns)
    {
        if (zone.EndColumn + 1 >= columns)
        {
            return false;
        }

        var t0 = tiles[zone.EndColumn + 0, zone.EndRow];
        var t1 = tiles[zone.EndColumn + 1, zone.EndRow];

        if (t0.Corners != t1.Corners)
        {
            return false;
        }

        if (!TileUtilities.AreSidesAligned(t0, t1, TileSide.East))
        {
            return false;
        }

        return true;
    }

    private static bool CanExpandSouth(IReadOnlyGrid<Tile> tiles, in Zone zone, int columns, int rows)
    {
        if (zone.EndRow + 1 >= rows)
        {
            return false;
        }

        var t0 = tiles[zone.EndColumn, zone.EndRow + 0];
        var t1 = tiles[zone.EndColumn, zone.EndRow + 1];

        if (t0.Corners != t1.Corners)
        {
            return false;
        }

        if (!TileUtilities.AreSidesAligned(t0, t1, TileSide.South))
        {
            return false;
        }


        return true;
    }
}
