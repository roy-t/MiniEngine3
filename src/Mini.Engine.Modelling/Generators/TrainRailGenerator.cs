using System.Numerics;
using Mini.Engine.Modelling.Tools;

namespace Mini.Engine.Modelling.Generators;
public static class TrainRailGenerator
{
    private const float SINGLE_RAIL_WIDTH_TOP = 0.1f;
    private const float SINGLE_RAIL_WIDTH_BOTTOM = 0.2f;
    private const float SINGLE_RAIL_HEIGTH = 0.2f;

    private const float SINGLE_RAIL_OFFSET = 0.65f + (SINGLE_RAIL_WIDTH_BOTTOM / 2.0f);

    public static Quad[] Generate()
    {        
        var crossSection = CreateSingleRailCrossSection();
        var trackLayout = CreateTrackLayout();

        var leftRailLayout = CreateSingleRailLayout(trackLayout, SINGLE_RAIL_OFFSET);
        var leftRail = Extruder.Extrude(crossSection, leftRailLayout);

        var rightRailLayout = CreateSingleRailLayout(trackLayout, -SINGLE_RAIL_OFFSET);
        var rightRail = Extruder.Extrude(crossSection, rightRailLayout);
        
        var quads = new Quad[leftRail.Length + rightRail.Length];
        Array.Copy(leftRail, 0, quads, 0, leftRail.Length);
        Array.Copy(rightRail, 0 ,quads, leftRail.Length, rightRail.Length);

        return quads;
    }

    private static Path3D CreateTrackLayout()
    {
        var layout = new Vector3[4];
        var step = (MathF.PI * 2.0f) / layout.Length;
        for (var i = 0; i < layout.Length; i++)
        {
            layout[i] = new Vector3(MathF.Cos(step * i), 0, MathF.Sin(step * i)) * 5.0f;
        }

        return new Path3D(true, layout);
    }


    private static Path3D CreateSingleRailLayout(Path3D trackLayout, float offset)
    {
        if (trackLayout.Length < 2)
        {
            throw new Exception("Invalid track layout");
        }

        var railLayout = new Vector3[trackLayout.Length];

        for (var i = 0; i < trackLayout.Length; i++)
        {
            var forward = trackLayout.GetForwardAlongBendToNextPosition(i);
            var up = Vector3.UnitY;
            var left = Vector3.Normalize(Vector3.Cross(up, forward));

            railLayout[i] = trackLayout[i] + left * offset;
        }

        return new Path3D(trackLayout.IsClosed, railLayout);
    }

    private static Path2D CreateSingleRailCrossSection()
    {
        return new Path2D(true, new Vector2(SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH), new Vector2(SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f), new Vector2(-SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f), new Vector2(-SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH));
    }
}
