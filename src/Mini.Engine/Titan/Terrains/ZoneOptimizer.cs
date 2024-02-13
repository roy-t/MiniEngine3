using LibGame.Mathematics;

namespace Mini.Engine.Titan.Terrains;

public readonly record struct Zone(int StartColumn, int EndColumn, int StartRow, int EndRow);

public sealed record class ZoneLookup(IReadOnlyList<int> Owners, IReadOnlyList<Zone> Zones);

public static class ZoneOptimizer
{
    public static ZoneLookup Optimize(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        var owners = new int[columns * rows];
        var zones = new List<Zone>();
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
                    var owner = zones.Count;
                    if (row > 0)
                    {
                        var northernZoneIndex = owners[Indexes.ToOneDimensional(zone.StartColumn, row - 1, columns)];
                        var northernZone = zones[northernZoneIndex];
                        if (northernZone.StartColumn == zone.StartColumn && northernZone.EndColumn == zone.EndColumn &&
                            CanExpandSouth(tiles, in northernZone, columns, rows))
                        {
                            owner = northernZoneIndex;
                            zones[northernZoneIndex] = new Zone(zone.StartColumn, zone.EndColumn, northernZone.StartRow, row);
                        }
                    }

                    for (var c = zone.StartColumn; c <= zone.EndColumn; c++)
                    {
                        owners[Indexes.ToOneDimensional(c, row, columns)] = owner;
                    }
                    if (owner == zones.Count)
                    {
                        zones.Add(zone);
                    }
                    zone = new Zone(zone.EndColumn + 1, zone.EndColumn + 1, row, row);
                }
            }
        }

        return new ZoneLookup(owners, zones);
    }

    private static bool CanExpandEast(IReadOnlyList<Tile> tiles, in Zone zone, int columns)
    {
        if (zone.EndColumn + 1 >= columns)
        {
            return false;
        }

        var t0 = tiles[Indexes.ToOneDimensional(zone.EndColumn + 0, zone.EndRow, columns)];
        var t1 = tiles[Indexes.ToOneDimensional(zone.EndColumn + 1, zone.EndRow, columns)];

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

    private static bool CanExpandSouth(IReadOnlyList<Tile> tiles, in Zone zone, int columns, int rows)
    {
        if (zone.EndRow + 1 >= rows)
        {
            return false;
        }

        var t0 = tiles[Indexes.ToOneDimensional(zone.EndColumn, zone.EndRow + 0, columns)];
        var t1 = tiles[Indexes.ToOneDimensional(zone.EndColumn, zone.EndRow + 1, columns)];

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
