using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;

public sealed class ZoneOptimizer
{
    public enum Modes
    {
        Explore,
        Catchup,
    }

    public readonly record struct Zone(int StartColumn, int EndColumn, int StartRow, int EndRow);

    public class State(int columns, int rows)
    {
        public int[] Owners { get; } = new int[columns * rows];
        public List<Zone> Zones { get; } = new List<Zone>();

        internal Modes Mode = Modes.Explore;
        internal int Back = 0;
        internal int Front = 0;
        internal int Zone = 0;
        internal bool MatchVertical = false;
    }

    public static State Optimize(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        var state = new State(columns, rows);
        for (var r = 0; r < rows; r++)
        {
            var offset = columns * r;
            state.Back = 0;
            state.Front = 0;

            while (state.Back < columns)
            {

                if (r > 0)
                {

                }
                switch (state.Mode)
                {
                    case Modes.Explore:

                        var canAdvance = CanAdvanceFront(in state, tiles, offset, columns);
                        if (canAdvance)
                        {
                            state.Front += 1;
                        }
                        else
                        {
                            state.MatchVertical = CanExpandNorthernZone(in state, tiles, offset, columns);
                            state.Mode = Modes.Catchup;
                        }

                        break;

                    case Modes.Catchup:
                        var zone = state.Zone;
                        if (state.MatchVertical)
                        {
                            zone = state.Owners[(state.Back + offset) - columns];
                        }
                        if (state.Back < state.Front || state.Front == (columns - 1))
                        {
                            state.Owners[state.Back + offset] = zone;
                            state.Back += 1;
                        }
                        else
                        {
                            state.Zone += state.MatchVertical ? 0 : 1;
                            state.Front += 1;
                            state.Mode = Modes.Explore;
                        }
                        break;
                }
            }
        }

        return state;
    }

    private static bool CanAdvanceFront(in State state, IReadOnlyList<Tile> tiles, int offset, int columns)
    {
        var hasNext = state.Front < (columns - 1);
        var back = tiles[state.Back + offset];
        var front = tiles[state.Front + offset];

        return front.Offset == back.Offset &&
            front.IsLevel() && back.IsLevel() && hasNext;
    }


    private static bool CanExpandNorthernZone(in State state, IReadOnlyList<Tile> tiles, int offset, int columns)
    {
        // There should be a row to the north
        var (_, y) = Indexes.ToTwoDimensional(state.Front + offset, columns);
        if (y == 0)
        {
            return false;
        }

        // Remember: front has already advanced to a position that doesn't match
        var f = (state.Front - 1);
        var b = state.Back;

        // Special case for 1x1 strips, they could be on a slope in which case we don't want to add them to a zone
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
        var zoneBackN = state.Owners[(b + offset) - columns];
        var zoneFrontN = state.Owners[(f + offset) - columns];

        if (zoneBackN != zoneFrontN)
        {
            return false;
        }

        // The zone NW of back should be different, if it exists
        if (b > 0)
        {
            var zoneBackNW = state.Owners[((b + offset) - columns) - 1];
            if (zoneBackNW == zoneBackN)
            {
                return false;
            }
        }

        // The zone NE of front should be different, if it exists
        if (f < (columns - 1))
        {
            var zoneFrontNE = state.Owners[((f + offset) - columns) + 1];
            if (zoneFrontNE == zoneFrontN)
            {
                return false;
            }
        }

        return true;
    }
}
