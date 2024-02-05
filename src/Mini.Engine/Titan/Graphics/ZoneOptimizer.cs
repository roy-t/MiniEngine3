using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;

public readonly record struct Zone(int StartColumn, int EndColumn, int StartRow, int EndRow);

public sealed record class ZoneLookup(IReadOnlyList<int> Owners, IReadOnlyList<Zone> Zones);

public sealed class ZoneOptimizer
{
    public static ZoneLookup Optimize(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        var owners = new int[columns * rows];
        var zones = new List<Zone>();
        var nextZone = 0;

        for (var r = 0; r < rows; r++)
        {
            var offset = columns * r;
            var back = 0;
            var front = 0;

            while (back < columns)
            {
                while (CanAdvanceFront(tiles, back, front, offset, columns))
                {
                    front += 1;
                }

                var zone = nextZone;
                var canExpand = CanExpandNorthernZone(tiles, owners, back, front, offset, columns);
                if (canExpand)
                {
                    zone = owners[(back + offset) - columns];
                }
                else
                {
                    zones.Add(new Zone());// TODO: update actual zones! Clean-up this zone stuff in a method!
                }

                while (CanAdvanceBack(back, front, columns))
                {
                    owners[back + offset] = zone;
                    back += 1;
                }

                nextZone += canExpand ? 0 : 1;
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
