using System.Diagnostics;
using LibGame.Mathematics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Titan.Graphics;

[Service]
public sealed class GreedyTerrainBuilder : ITerrainBuilder
{
    private const int Unowned = -1;

    // Either a zone is 1x1 size, in which case we should look up the tile and create a separate mesh for it,
    // Or the zone is larger and we can look at any tile in the zone to figure out height and mesh it as a whole.
    private readonly record struct Zone(int ColMin, int ColMax, int RowMin, int RowMax);


    public enum Mode
    {
        Explore,
        Catchup,
    }

    private class State(int columns, int rows)
    {
        public Mode Mode = Mode.Explore;
        public int Front = 0;
        public int Back = 0;
        public int Zone = 1;
        public int[] Owners = new int[columns * rows];
    }

    public static int[] Build2(Tile[] tiles, int columns, int rows)
    {
        Debug.Assert(columns > 0);
        Debug.Assert(rows > 0);

        var state = new State(columns, rows);

        // TODO: this could work? Try as a colorizer first?

        // Treat first column seperately
        while (state.Front < columns)
        {
            switch (state.Mode)
            {
                case Mode.Explore:
                    var back = tiles[state.Back];
                    var front = tiles[state.Front];
                    if (front.IsLevel() && front.Offset == back.Offset)
                    {
                        state.Front += 1;
                    }
                    else
                    {
                        state.Mode = Mode.Catchup;
                    }
                    break;

                case Mode.Catchup:
                    if (state.Back <= state.Front)
                    {
                        state.Owners[state.Back] = state.Zone;

                    }
                    if (state.Back == state.Front)
                    {
                        state.Mode = Mode.Explore;
                        state.Front += 1;
                        state.Zone += 1;
                    }
                    state.Back += 1;
                    break;
            }
        }

        throw new NotImplementedException();
    }


    public TerrainMesh Build(Tile[] tiles, ITerrainColorizer colorizer, int columns, int rows)
    {
        // TODO: we might be able to get rid of initializing this array if we offset
        // indexing in other places, but for convenience lets leave it at this for now
        var owners = new int[tiles.Length];
        for (var i = 0; i < owners.Length; i++)
        {
            owners[i] = Unowned;
        }
        var zones = new List<Zone>();
        var vertices = new List<TerrainVertex>();

        var started = false;
        var column = 0;
        for (var i = 0; i < tiles.Length; i++)
        {
            var (c, r) = Indexes.ToTwoDimensional(i, columns);

            if (owners[i] == Unowned) { continue; }
            if (tiles[i].IsLevel() == false)
            {
                if (started)
                {
                    zones.Add(new Zone(column, c - 1, r, r));
                    started = false;
                }
                zones.Add(new Zone(c, c, r, r));
            }

            // If we started a new row, start a new zone
            if (c == 0)
            {
                if (started)
                {
                    zones.Add(new Zone(column, columns - 1, (r - 1), (r - 1)));
                    started = false;
                }
                zones.Add(new Zone(c, c, r, r));
            }
        }

        throw new NotImplementedException();
    }
}
