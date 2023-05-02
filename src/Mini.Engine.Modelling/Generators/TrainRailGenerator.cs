using System.Numerics;
using Mini.Engine.Modelling.Tools;

namespace Mini.Engine.Modelling.Generators;
public static class TrainRailGenerator
{
    public static Quad[] Generate()
    {
        var crossSection = CreateSingleRailCrossSection();
        var layout = CreateTrackLayout();
        var quads = Extruder.Extrude(crossSection, layout, true);

        return quads;
    }

    private static Vector3[] CreateTrackLayout()
    {
        return new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(1, 0.5f, -5) };
        //return new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 0, -2) };
    }

    private static Shape CreateSingleRailCrossSection()
    {
        var railTopWidth = 0.1f;
        var railBottomWidth = 0.2f;
        var railHeigth = 0.2f;

        return new Shape(new Vector2(railTopWidth / 2.0f, railHeigth), new Vector2(railBottomWidth / 2.0f, 0.0f), new Vector2(-railBottomWidth / 2.0f, 0.0f), new Vector2(-railTopWidth / 2.0f, railHeigth));
    }
}
