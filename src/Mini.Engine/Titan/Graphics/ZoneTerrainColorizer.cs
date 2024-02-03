using System.Diagnostics;
using LibGame.Graphics;

namespace Mini.Engine.Titan.Graphics;
public sealed class ZoneTerrainColorizer : ITerrainColorizer
{
    public enum Modes
    {
        Explore,
        Catchup,
    }

    private readonly ColorPalette ColorPalette;

    private readonly int[] Owners;
    private Modes mode;
    private int back;
    private int front;
    private int zone;
    private bool matchAbove;

    public ZoneTerrainColorizer(Tile[] tiles, int columns, int rows)
    {
        this.ColorPalette = ColorPalette.GrassLawn;

        this.Owners = new int[columns * rows];
        this.mode = Modes.Explore;
        this.back = 0;
        this.front = 0;
        this.zone = 0;

        this.Ininitialize(tiles, columns, rows);
    }

    private void Ininitialize(Tile[] tiles, int columns, int rows)
    {
        Debug.Assert(columns > 0);
        Debug.Assert(rows > 0);

        // Treat first row seperately as we don't have to check if we can expand the zone in the previous row
        while (this.back < columns)
        {
            switch (this.mode)
            {
                case Modes.Explore:
                    var back = tiles[this.back];
                    var front = tiles[this.front];

                    var eol = this.front >= (columns - 1);
                    var connected = front.Offset == back.Offset &&
                        front.IsLevel() && back.IsLevel();

                    if (eol)
                    {
                        this.front += 1;
                        this.mode = Modes.Catchup;
                    }
                    else if (connected)
                    {
                        this.front += 1;
                    }
                    else
                    {

                        this.mode = Modes.Catchup;
                    }

                    break;

                case Modes.Catchup:
                    if (this.back < this.front)
                    {
                        this.Owners[this.back] = this.zone;
                        this.back += 1;
                    }
                    else
                    {
                        this.zone += 1;
                        this.front += 1;
                        this.mode = Modes.Explore;
                    }
                    break;
            }
        }

        for (var r = 1; r < rows; r++)
        {
            var offset = columns * r;
            this.back = 0;
            this.front = 0;

            while (this.back < columns)
            {
                switch (this.mode)
                {
                    case Modes.Explore:
                        var back = tiles[this.back + offset];
                        var front = tiles[this.front + offset];

                        var eol = this.front >= (columns - 1);
                        var connected = front.Offset == back.Offset &&
                            front.IsLevel() && back.IsLevel();

                        if (connected && !eol)
                        {
                            this.front += 1;
                        }
                        else
                        {
                            var zoneBackN = this.Owners[(this.back + offset) - columns];
                            var zoneFrontN = this.Owners[(this.front + offset) - columns];
                            this.matchAbove = zoneBackN == zoneFrontN;


                            if (this.back > 0)
                            {
                                var zoneBackNW = this.Owners[((this.back + offset) - columns) - 1];
                                this.matchAbove = this.matchAbove && (zoneBackNW != zoneBackN);
                            }
                            if (this.front < (columns - 1))
                            {
                                var zoneFrontNE = this.Owners[((this.front + offset) - columns) + 1];
                                this.matchAbove = this.matchAbove && (zoneFrontNE != zoneBackN);
                            }

                            this.mode = Modes.Catchup;
                            if (eol) { this.front += 1; }
                        }

                        break;

                    case Modes.Catchup:
                        var zone = this.zone;
                        if (this.matchAbove)
                        {
                            zone = this.Owners[(this.back + offset) - columns];
                        }
                        if (this.back < this.front)
                        {
                            this.Owners[this.back + offset] = zone;
                            this.back += 1;
                        }
                        else
                        {
                            this.zone += this.matchAbove ? 0 : 1;
                            this.front += 1;
                            this.mode = Modes.Explore;
                        }
                        break;
                }
            }
        }
    }

    public ColorLinear GetColor(IReadOnlyList<Tile> tiles, int i, IReadOnlyList<TerrainVertex> vertices, int a, int b, int c)
    {
        var owner = this.Owners[i];
        var index = owner % this.ColorPalette.Colors.Count;
        return Colors.RGBToLinear(this.ColorPalette.Colors[index]);
    }
}
