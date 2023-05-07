using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;
using Mini.Engine.Modelling.Tools;

namespace Mini.Engine.Modelling.Generators;
public static class TrainRailGenerator
{
    private const float SINGLE_RAIL_WIDTH_TOP = 0.1f;
    private const float SINGLE_RAIL_WIDTH_BOTTOM = 0.2f;
    private const float SINGLE_RAIL_HEIGTH = 0.2f;
    private const float SINGLE_RAIL_OFFSET = 0.65f + (SINGLE_RAIL_WIDTH_BOTTOM / 2.0f);

    private const float BALLAST_WIDTH_TOP = 3.0f;
    private const float BALLAST_WIDTH_MIDDLE = 4.0f;
    private const float BALLAST_WIDTH_BOTTOM = 6.0f;

    private const float BALLAST_HEIGHT_TOP = 1.0f;
    private const float BALLAST_HEIGHT_MIDDLE = 0.5f;
    private const float BALLAST_HEIGHT_BOTTOM = 0.0f;


    public static Quad[] GenerateRails(Path3D trackLayout)
    {
        var crossSection = CreateSingleRailCrossSection();

        var leftRailLayout = CreateSingleRailLayout(trackLayout, SINGLE_RAIL_OFFSET);
        var leftRail = Extruder.Extrude(crossSection, leftRailLayout);

        var rightRailLayout = CreateSingleRailLayout(trackLayout, -SINGLE_RAIL_OFFSET);
        var rightRail = Extruder.Extrude(crossSection, rightRailLayout);

        var caps = CreateRailEndCaps(leftRailLayout, rightRailLayout);

        return ArrayUtilities.Concat(leftRail, rightRail, caps);
    }

    public static Quad[] GenerateBallast(Path3D trackLayout)
    {
        var crossSection = CreateBallastCrossSection();
        var ballast = Extruder.Extrude(crossSection, trackLayout);

        var caps = CreateBallastEndCaps(trackLayout);

        return ArrayUtilities.Concat(ballast, caps);
    }

    public static Path3D CreateTrackLayout()
    {
        var layout = new Vector3[40];
        var step = (MathF.PI * 2.0f) / layout.Length;
        for (var i = 0; i < layout.Length; i++)
        {
            layout[i] = new Vector3(MathF.Cos(step * i), 0, MathF.Sin(step * i)) * 5.0f;
        }

        return new Path3D(false, layout);
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
        var topRight = new Vector2(SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH);
        var bottomRight = new Vector2(SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f);
        var bottomLeft = new Vector2(-SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f);
        var topLeft = new Vector2(-SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH);

        var h = new Vector2(0, BALLAST_HEIGHT_TOP);

        return new Path2D(true, topRight + h, bottomRight + h, bottomLeft + h, topLeft + h);
    }

    private static Quad[] CreateRailEndCaps(params Path3D[] railLayouts)
    {
        var caps = new Quad[railLayouts.Length * 2];

        for (var i = 0; i < railLayouts.Length; i++)
        {
            var layout = railLayouts[i];
            caps[i * 2] = CreateSingleRailEndCap(layout[0], layout.GetForward(0));
            caps[(i * 2) + 1] = CreateSingleRailEndCap(layout[layout.Length - 1], layout.GetBackward(layout.Length - 1));
        }

        return caps;
    }

    private static Quad CreateSingleRailEndCap(Vector3 position, Vector3 direction)
    {
        var topRight = new Vector3(SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH, 0.0f);
        var bottomRight = new Vector3(SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f, 0.0f);
        var bottomLeft = new Vector3(-SINGLE_RAIL_WIDTH_BOTTOM / 2.0f, 0.0f, 0.0f);
        var topLeft = new Vector3(-SINGLE_RAIL_WIDTH_TOP / 2.0f, SINGLE_RAIL_HEIGTH, 0.0f);


        var h = new Vector3(0, BALLAST_HEIGHT_TOP, 0.0f);

        var quad = new Quad(Vector3.UnitZ, topRight + h, bottomRight + h, bottomLeft + h, topLeft + h);

        var transform = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f);
        transform = transform.FaceTargetConstrained(position + direction, Vector3.UnitY);

        return quad.CreateTransformed(in transform);
    }

    private static Path2D CreateBallastCrossSection()
    {
        var topRight = new Vector2(BALLAST_WIDTH_TOP / 2.0f, BALLAST_HEIGHT_TOP);
        var middleRight = new Vector2(BALLAST_WIDTH_MIDDLE / 2.0f, BALLAST_HEIGHT_MIDDLE);
        var bottomRight = new Vector2(BALLAST_WIDTH_BOTTOM / 2.0f, BALLAST_HEIGHT_BOTTOM);

        var bottomLeft = new Vector2(-BALLAST_WIDTH_BOTTOM / 2.0f, BALLAST_HEIGHT_BOTTOM);
        var middleLeft = new Vector2(-BALLAST_WIDTH_MIDDLE / 2.0f, BALLAST_HEIGHT_MIDDLE);
        var topLeft = new Vector2(-BALLAST_WIDTH_TOP / 2.0f, BALLAST_HEIGHT_TOP);

        return new Path2D(true, topRight, middleRight, bottomRight, bottomLeft, middleLeft, topLeft);
    }

    private static Quad[] CreateBallastEndCaps(Path3D trackLayout)
    {
        return new Quad[4]
        {
            CreateTopBallastEndCap(trackLayout[0], trackLayout.GetForward(0)),
            CreateBottomBallastEndCap(trackLayout[0], trackLayout.GetForward(0)),
            CreateTopBallastEndCap(trackLayout[trackLayout.Length - 1], trackLayout.GetBackward(trackLayout.Length - 1)),
            CreateBottomBallastEndCap(trackLayout[trackLayout.Length - 1], trackLayout.GetBackward(trackLayout.Length - 1)),
        };
    }

    private static Quad CreateTopBallastEndCap(Vector3 position, Vector3 direction)
    {
        var crossSection = CreateBallastCrossSection();        
        var topRight = new Vector3(crossSection[0], 0);
        var bottomRight = new Vector3(crossSection[1], 0);
        var bottomLeft = new Vector3(crossSection[4], 0);
        var topLeft = new Vector3(crossSection[5], 0);
        
        var quad = new Quad(Vector3.UnitZ, topRight, bottomRight, bottomLeft, topLeft);

        var transform = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f);
        transform = transform.FaceTargetConstrained(position + direction, Vector3.UnitY);

        return quad.CreateTransformed(in transform);
    }

    private static Quad CreateBottomBallastEndCap(Vector3 position, Vector3 direction)
    {
        var crossSection = CreateBallastCrossSection();
        var topRight = new Vector3(crossSection[1], 0);
        var bottomRight = new Vector3(crossSection[2], 0);
        var bottomLeft = new Vector3(crossSection[3], 0);
        var topLeft = new Vector3(crossSection[4], 0);

        var quad = new Quad(Vector3.UnitZ, topRight, bottomRight, bottomLeft, topLeft);

        var transform = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f);
        transform = transform.FaceTargetConstrained(position + direction, Vector3.UnitY);

        return quad.CreateTransformed(in transform);
    }
}

