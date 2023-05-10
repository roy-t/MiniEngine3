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

    private const float RAIL_TIE_WIDTH = 2.0f;
    private const float RAIL_TIE_HEIGHT = 0.15f;
    private const float RAIL_TIE_DEPTH = 0.2f;
    private const float RAIL_TIE_SPACING = 0.3f + RAIL_TIE_DEPTH;

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

        return ballast;
        //var caps = CreateBallastEndCaps(trackLayout);
        //return ArrayUtilities.Concat(ballast, caps);
    }

    public static (Quad[], Matrix4x4[]) GenerateRailTies(Path3D trackLayout)
    {
        var quads = CreateSingleRailTie();
        var transforms = Walker.Walk(trackLayout, RAIL_TIE_SPACING);
        var matrices = transforms.Select(t => t.GetMatrix()).ToArray();

        return (quads, matrices);
    }

    public static Path3D CreateTrackLayout()
    {
        var layout = new Vector3[10];
        var step = (MathF.PI * 2.0f) / layout.Length;
        for (var i = 0; i < layout.Length; i++)
        {
            layout[i] = new Vector3(MathF.Cos(step * i), 0, MathF.Sin(step * i)) * 5.0f;
        }

        return new Path3D(true, layout); // TODO: closed lop true creates really weird rail and ballast
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

            railLayout[i] = trackLayout[i] + (left * offset);
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
            caps[(i * 2) + 0] = CreateSingleRailEndCap(layout[0], layout.GetForward(0));
            caps[(i * 2) + 1] = CreateSingleRailEndCap(layout[layout.Length - 1], layout.GetBackward(layout.Length - 1));
        }

        return caps;
    }

    private static Quad CreateSingleRailEndCap(Vector3 position, Vector3 direction)
    {
        var crossSection = CreateSingleRailCrossSection();        
        var quad = Quad.SingleFromPath(crossSection);
        
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
        var start = CreateBallastEndCap(trackLayout[0], trackLayout.GetForward(0));
        var end = CreateBallastEndCap(trackLayout[trackLayout.Length - 1], trackLayout.GetBackward(trackLayout.Length - 1));


        return ArrayUtilities.Concat(start, end);        
    }

    private static Quad[] CreateBallastEndCap(Vector3 position, Vector3 direction)
    {
        var crossSection = CreateBallastCrossSection();
        var quads = Quad.MultipleFromPath(crossSection, 0, 1, 4, 5, 1, 2, 3, 4);

        for (var i = 0; i < quads.Length; i++)
        {
            var transform = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f);
            transform = transform.FaceTargetConstrained(position + direction, Vector3.UnitY);

            quads[i] = quads[i].CreateTransformed(in transform);
        }


        return quads;
    }

    private static Quad[] CreateSingleRailTie()
    {
        var h = new Vector3(0, BALLAST_HEIGHT_TOP, 0);

        var topRightA = new Vector3(RAIL_TIE_WIDTH / 2.0f, RAIL_TIE_HEIGHT, RAIL_TIE_DEPTH / 2.0f) + h;
        var bottomRightA = new Vector3(RAIL_TIE_WIDTH / 2.0f, 0.0f, RAIL_TIE_DEPTH / 2.0f) + h;
        var bottomLeftA = new Vector3(-RAIL_TIE_WIDTH / 2.0f, 0.0f, RAIL_TIE_DEPTH / 2.0f) + h;
        var topLeftA = new Vector3(-RAIL_TIE_WIDTH / 2.0f, RAIL_TIE_HEIGHT, RAIL_TIE_DEPTH / 2.0f) + h;

        var front = new Path3D(true, topRightA, bottomRightA, bottomLeftA, topLeftA);

        var topRightB = new Vector3(RAIL_TIE_WIDTH / 2.0f, RAIL_TIE_HEIGHT, -RAIL_TIE_DEPTH / 2.0f) + h;
        var bottomRightB = new Vector3(RAIL_TIE_WIDTH / 2.0f, 0.0f, -RAIL_TIE_DEPTH / 2.0f) + h;
        var bottomLeftB = new Vector3(-RAIL_TIE_WIDTH / 2.0f, 0.0f, -RAIL_TIE_DEPTH / 2.0f) + h;
        var topLeftB = new Vector3(-RAIL_TIE_WIDTH / 2.0f, RAIL_TIE_HEIGHT, -RAIL_TIE_DEPTH / 2.0f) + h;

        var back = new Path3D(true, topRightB, bottomRightB, bottomLeftB, topLeftB);

        return Joiner.Join(front, back);
    }
}

