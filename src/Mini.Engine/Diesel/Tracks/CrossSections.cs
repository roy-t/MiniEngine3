using System.Numerics;
using Mini.Engine.Modelling.Paths;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;
public static class CrossSections
{
    public static Path2D RailCrossSection()
    {
        var topRight = new Vector2(SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH);
        var bottomRight = new Vector2(SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f);
        var bottomLeft = new Vector2(-SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f);
        var topLeft = new Vector2(-SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH);

        var h = new Vector2(0, BALLAST_HEIGHT_TOP);

        return new Path2D(true, topRight + h, bottomRight + h, bottomLeft + h, topLeft + h);
    }

    public static Path3D TieCrossSectionFront()
    {
        var path = TieCrossSectionArray();       
        return new Path3D(true, path);
    }

    public static Path3D TieCrossSectionBack()
    {
        var path = TieCrossSectionArray();

        for (var i = 0; i < path.Length; i++)
        {
            var vertex = path[i];
            path[i] = new Vector3(vertex.X, vertex.Y, -vertex.Z);
        }

        return new Path3D(true, path);
    }

    private static Vector3[] TieCrossSectionArray()
    {
        var h = new Vector3(0, BALLAST_HEIGHT_TOP, 0);

        var halfDepthTop = RAIL_TIE_DEPTH_TOP / 2.0f;
        var halfDepthBottom = RAIL_TIE_DEPTH_BOTTOM / 2.0f;
        var halfWidthTop = RAIL_TIE_WIDTH / 2.0f;
        var halfWidthBottom = RAIL_TIE_WIDTH_BOTTOM / 2.0f;

        var front = new Vector3[]
        {
            new Vector3(halfWidthTop, RAIL_TIE_HEIGHT, halfDepthTop) + h,
            new Vector3(halfWidthBottom, 0.0f, halfDepthBottom) + h,
            new Vector3(0.0f, 0.0f, halfDepthBottom) + h,
            new Vector3(-halfWidthBottom, 0.0f, halfDepthBottom) + h,
            new Vector3(-halfWidthTop, RAIL_TIE_HEIGHT, halfDepthTop) + h,
            new Vector3(0.0f, RAIL_TIE_MID_HEIGHT, halfDepthTop) + h,
        };

        return front;
    }

}
