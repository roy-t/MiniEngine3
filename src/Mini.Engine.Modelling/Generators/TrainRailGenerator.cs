using System.Numerics;
using Mini.Engine.Modelling.Tools;

namespace Mini.Engine.Modelling.Generators;
public static class TrainRailGenerator
{
    public static Quad[] Generate()
    {
        var crossSection = CreateSingleRailCrossSection();
        var quads = Extruder.Extrude(crossSection, 10.0f);

        return quads;
    }

    private static Shape CreateSingleRailCrossSection()
    {
        var railTopWidth = 0.1f;
        var railBottomWidth = 0.2f;
        var railHeigth = 0.2f;

        return new Shape(new Vector2(railTopWidth / 2.0f, railHeigth), new Vector2(railBottomWidth / 2.0f, 0.0f), new Vector2(-railBottomWidth / 2.0f, 0.0f), new Vector2(-railTopWidth / 2.0f, railHeigth));
    }
}
