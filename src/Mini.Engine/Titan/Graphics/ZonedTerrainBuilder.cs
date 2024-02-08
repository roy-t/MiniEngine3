//using LibGame.Mathematics;

//namespace Mini.Engine.Titan.Graphics;

//public sealed class ZonedTerrainBuilder : ITerrainBuilder
//{
//    private const int UNSET = -1;

//    public ZonedTerrainBuilder()
//    {
//    }

//    public TerrainMesh Build(Tile[] tiles, ITerrainColorizer colorizer, int columns, int rows)
//    {
//        var (owners, zones) = ZoneOptimizer.OptimizeFlat(tiles, columns, rows);

//        var vertexLookup = new int[(columns + 1) * (rows + 1)];
//        for (var i = 0; i < vertexLookup.Length; i++)
//        {
//            vertexLookup[i] = UNSET;
//        }
//        var vertices = new List<TerrainVertex>(zones.Count * 2);
//        var indices = new List<int>();
//        for (var i = 0; i < zones.Count; i++)
//        {
//            var zone = zones[i];
//            var tli = GetCornerIndex(zone.StartRow, zone.StartRow, TileCorner.NW, columns + 1);
//            if (HasVertex(vertexLookup, tli, out var tlv))
//            {
//                indices.Add(tlv);
//            }
//            else
//            {
//                vertices.Add // NEW VERTEX
//                    // SET LOOKUP
//                    // ADD INDEX
//            }
//            // REPEAT FOR EACH CORNER
//        }

//        throw new NotImplementedException();
//    }


//    private static int GetCornerIndex(int column, int row, TileCorner corner, int stride)
//    {
//        var (x, y) = corner switch
//        {
//            TileCorner.NE => (column + 1, row + 0),
//            TileCorner.SE => (column + 1, row + 1),
//            TileCorner.SW => (column + 0, row + 1),
//            TileCorner.NW => (column + 0, row + 0),
//            _ => throw new ArgumentOutOfRangeException(nameof(corner))
//        };

//        return Indexes.ToOneDimensional(x, y, stride);
//    }

//    private static bool HasVertex(int[] vertexLookUp, int index, out int i)
//    {
//        var vertex = vertexLookUp[index];
//        if (vertex == UNSET)
//        {
//            i = -1;
//            return false;
//        }

//        i = vertex;
//        return true;
//    }
//}
