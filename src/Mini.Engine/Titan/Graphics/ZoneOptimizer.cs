using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;

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
                        // TODO: in some cases the zone is not expanded as far south as it could?
                        throw new Exception("TODO");
                        var northernZoneIndex = owners[Indexes.ToOneDimensional(zone.StartColumn, row - 1, columns)];
                        var northernZone = zones[northernZoneIndex];
                        if (northernZone.StartColumn == zone.StartColumn && northernZone.EndColumn == zone.EndColumn
                            && CanExpandSouth(tiles, in zone, columns, rows))
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

        if (!TileUtilities.AreSidesAligned(t0, t1, TileSide.East))
        {
            return false;
        }

        return TileUtilities.AreNormalsAligned(t0, t1);
    }

    private static bool CanExpandSouth(IReadOnlyList<Tile> tiles, in Zone zone, int columns, int rows)
    {
        if (zone.EndRow + 1 >= rows)
        {
            return false;
        }

        var t0 = tiles[Indexes.ToOneDimensional(zone.EndColumn, zone.EndRow + 0, columns)];
        var t1 = tiles[Indexes.ToOneDimensional(zone.EndColumn, zone.EndRow + 1, columns)];

        if (!TileUtilities.AreSidesAligned(t0, t1, TileSide.South))
        {
            return false;
        }


        return TileUtilities.AreNormalsAligned(t0, t1))        
    }



    public static ZoneLookup OptimizeFlat(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        var owners = new int[columns * rows];
        var zones = new List<Zone>();

        for (var r = 0; r < rows; r++)
        {
            var offset = columns * r;
            var back = 0; // inclusive start of connected tiles
            var front = 0; // exclusive end of connected tile

            while (back < columns)
            {
                while (CanAdvanceFront(tiles, back, front, offset, columns))
                {
                    front += 1;
                }

                var zoneId = zones.Count;
                var canExpand = CanExpandNorthernZone(tiles, owners, back, front, offset, columns);
                if (canExpand)
                {
                    zoneId = owners[(back + offset) - columns];
                    zones[zoneId] = zones[zoneId] with { EndRow = r };
                }
                else
                {
                    zones.Add(new Zone(back, front - 1, r, r));// TODO: update actual zones! Clean-up this zone stuff in a method!
                }

                while (CanAdvanceBack(back, front, columns))
                {
                    owners[back + offset] = zoneId;
                    back += 1;
                }

                front += 1;
            }
        }

        return new ZoneLookup(owners, zones);
    }

    private static bool CanAdvanceBack(int back, int front, int columns)
    {
        return (back < columns) && ((back < front) || (front == (columns - 1)));
    }

    private static bool CanAdvanceFront(IReadOnlyList<Tile> tiles, int back, int front, int offset, int columns)
    {
        if (front >= columns)
        {
            return false;
        }

        var b = tiles[back + offset];
        var f = tiles[front + offset];

        return f.Offset == b.Offset &&
               f.IsLevel() && b.IsLevel();
    }

    private static bool CanExpandNorthernZone(IReadOnlyList<Tile> tiles, IList<int> Owners, int back, int front, int offset, int columns)
    {
        // There should be a row to the north
        var (_, y) = Indexes.ToTwoDimensional(back + offset, columns);
        if (y == 0)
        {
            return false;
        }

        // Remember: front has already advanced to a position that doesn't match
        var f = front - 1;
        var b = back;

        // Special case for 1x1 strips, they could be on a slope in which case we don't want to add them to any other zone
        if (b == f)
        {
            var t = tiles[f + offset];
            if (!t.IsLevel())
            {
                return false;
            }
            var n = tiles[f + offset - columns];
            if (!n.IsLevel())
            {
                return false;
            }
            if (t.Offset != n.Offset)
            {
                return false;
            }
        }

        // The zone north of our front and back should be the same
        var zoneBackN = Owners[(b + offset) - columns];
        var zoneFrontN = Owners[(f + offset) - columns];

        if (zoneBackN != zoneFrontN)
        {
            return false;
        }

        // The zone NW of back should be different, if it exists
        if (b > 0)
        {
            var zoneBackNW = Owners[((b + offset) - columns) - 1];
            if (zoneBackNW == zoneBackN)
            {
                return false;
            }
        }

        // The zone NE of front should be different, if it exists
        if (f < (columns - 1))
        {
            var zoneFrontNE = Owners[((f + offset) - columns) + 1];
            if (zoneFrontNE == zoneFrontN)
            {
                return false;
            }
        }

        return true;
    }
}
