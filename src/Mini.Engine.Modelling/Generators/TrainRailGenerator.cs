using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Tools;
using Vortice.Mathematics;

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
    private const float RAIL_TIE_WIDTH_BOTTOM = 2.2f;
    private const float RAIL_TIE_HEIGHT = 0.15f;
    private const float RAIL_TIE_MID_HEIGHT = 0.05f;
    private const float RAIL_TIE_DEPTH_TOP = 0.2f;
    private const float RAIL_TIE_DEPTH_BOTTOM = 0.3f;
    private const float RAIL_TIE_SPACING = 0.3f + RAIL_TIE_DEPTH_BOTTOM;

    public static Path3D CreateCircularTrackLayout()
    {
        var closed = false;
        var radius = 30.0f;
        var startAngle = MathF.PI * 0.0f;
        var endAngle = MathF.PI * 2.0f;
        var points = 50;
        // var vertices = PathUtilities.CreateCurve(radius, startAngle, endAngle, points, closed).Select(v => new Vector3(v.X, 0, v.Y)).ToArray();


        var curve = new CircularArcCurve(startAngle, endAngle, radius, closed);
        var vertices = Enumerable.Range(0, points)
            .Select(i => curve.GetPosition(i / (float)(points - 1.0f)))
            .Select(v => new Vector3(v.X, 0.0f, v.Y))
            .ToArray();

        return new Path3D(closed, vertices);
    }

    public static void CreateTurn(PrimitiveMeshBuilder builder)
    {        
        // in the future we can use a second curve to get the height
        
        var radius = 25.0f;
        var points = 50;

        var curve = new CircularArcCurve(0.0f, MathF.PI / 2.0f, radius);
        //var curve = PolynomialCurve.CreateTransitionCurve(radius);
        GenerateBallast(builder, curve, points, new Color4(0.33f, 0.27f, 0.25f, 1.0f));
        GenerateRailTies(builder, curve, new Color4(0.4f, 0.4f, 0.4f, 1.0f));
        GenerateRails(builder, curve, points, new Color4(0.4f, 0.28f, 0.30f, 1.0f));

    }

    private static void GenerateRails(PrimitiveMeshBuilder builder, ICurve curve, int points, Color4 color)
    {
        var crossSection = CreateSingleRailCrossSection();

        var rails = new ArrayBuilder<Quad>(crossSection.Steps * (points - 1) * 2);

        rails.Add(Extruder.Extrude(crossSection, curve, points, Vector3.UnitY, Vector3.UnitX * SINGLE_RAIL_OFFSET));
        rails.Add(Extruder.Extrude(crossSection, curve, points, Vector3.UnitY, Vector3.UnitX * -SINGLE_RAIL_OFFSET));        
        rails.Add(CreateRailEndCaps(curve, SINGLE_RAIL_OFFSET, -SINGLE_RAIL_OFFSET));

        var span = rails.Build();
        var vertices = QuadBuilder.GetVertices(span);
        var indices = QuadBuilder.GetIndices(span);

        builder.Add(vertices, indices, color);
    }

    private static void GenerateBallast(PrimitiveMeshBuilder builder, ICurve curve, int points, Color4 color)
    {
        var crossSection = CreateBallastCrossSection();        

        var ballast = new ArrayBuilder<Quad>((points - 1) * crossSection.Steps);

        ballast.Add(Extruder.Extrude(crossSection, curve, points, Vector3.UnitY, Vector3.Zero));

        var caps = CreateBallastEndCaps(curve);
        ballast.Add(caps);

        var span = ballast.Build();
        var vertices = QuadBuilder.GetVertices(span);
        var indices = QuadBuilder.GetIndices(span);

        builder.Add(vertices, indices, color);
    }

    private static void GenerateRailTies(PrimitiveMeshBuilder builder, ICurve curve, Color4 color)
    {
        var tie = CreateSingleRailTie();
        var transforms = Walker.WalkSpacedOut(curve, RAIL_TIE_SPACING, Vector3.UnitY);
        var matrices = transforms.Select(t => t.GetMatrix()).ToArray();

        var ties = new ArrayBuilder<Quad>(tie.Length * transforms.Length);

        for (var i = 0; i < transforms.Length; i++)
        {
            for (var q = 0; q < tie.Length; q++)
            {
                ties.Add(tie[q].CreateTransformed(in transforms[i]));
            }
        }

        var span = ties.Build();
        var vertices = QuadBuilder.GetVertices(span);
        var indices = QuadBuilder.GetIndices(span);

        builder.Add(vertices, indices, color);
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

    private static Quad[] CreateRailEndCaps(ICurve curve, params float[] offsets)
    {
        var caps = new Quad[offsets.Length * 2];

        for (var i = 0; i < offsets.Length; i++)
        {
            var offset = offsets[i];
            
            var n0 = curve.GetNormal3D(0);
            var l0 = curve.GetLeft(0);
            var p0 = curve.GetPosition3D(0) + (l0 * offset);
            
            var n1 = curve.GetNormal3D(1);
            var l1 = curve.GetLeft(1);
            var p1 = curve.GetPosition3D(1) + (l1 * offset);

            caps[(i * 2) + 0] = CreateSingleRailEndCap(p0, n0);
            caps[(i * 2) + 1] = CreateSingleRailEndCap(p1, -n1);
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

    private static Quad[] CreateBallastEndCaps(ICurve curve)
    {
        var p0 = curve.GetPosition3D(0);
        var n0 = curve.GetNormal3D(0);
        var start = CreateBallastEndCap(p0, n0);

        var p1 = curve.GetPosition3D(1);
        var n1 = curve.GetNormal3D(1);
        var end = CreateBallastEndCap(p1, -n1);

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

        var halfDepthTop = RAIL_TIE_DEPTH_TOP / 2.0f;
        var halfDepthBottom = RAIL_TIE_DEPTH_BOTTOM / 2.0f;
        var halfWidthTop = RAIL_TIE_WIDTH / 2.0f;
        var halfWidthBottom = RAIL_TIE_WIDTH_BOTTOM / 2.0f;

        var topRightA = new Vector3(halfWidthTop, RAIL_TIE_HEIGHT, halfDepthTop) + h;
        var bottomRightA = new Vector3(halfWidthBottom, 0.0f, halfDepthBottom) + h;
        var bottomMidA = new Vector3(0.0f, 0.0f, halfDepthBottom) + h;
        var bottomLeftA = new Vector3(-halfWidthBottom, 0.0f, halfDepthBottom) + h;
        var topLeftA = new Vector3(-halfWidthTop, RAIL_TIE_HEIGHT, halfDepthTop) + h;
        var topMidA = new Vector3(0.0f, RAIL_TIE_MID_HEIGHT, halfDepthTop) + h;

        var front = new Path3D(true, topRightA, bottomRightA, bottomMidA, bottomLeftA, topLeftA, topMidA);

        var topRightB = new Vector3(halfWidthTop, RAIL_TIE_HEIGHT, -halfDepthTop) + h;
        var bottomRightB = new Vector3(halfWidthBottom, 0.0f, -halfDepthBottom) + h;
        var bottomMidB = new Vector3(0.0f, 0.0f, -halfDepthBottom) + h;
        var bottomLeftB = new Vector3(-halfWidthBottom, 0.0f, -halfDepthBottom) + h;
        var topLeftB = new Vector3(-halfWidthTop, RAIL_TIE_HEIGHT, -halfDepthTop) + h;
        var topMidB = new Vector3(0.0f, RAIL_TIE_MID_HEIGHT, -halfDepthTop) + h;

        var back = new Path3D(true, topRightB, bottomRightB, bottomMidB, bottomLeftB, topLeftB, topMidB);

        var caps = new Quad[]
        {
            Quad.SingleFromPath(front, 0, 1, 2, 5),
            Quad.SingleFromPath(front, 5, 2, 3, 4),
            Quad.SingleFromPath(back, 5, 2, 1, 0),
            Quad.SingleFromPath(back, 4, 3, 2, 5),
        };

        return ArrayUtilities.Concat(Joiner.Join(front, back), caps);
    }
}

