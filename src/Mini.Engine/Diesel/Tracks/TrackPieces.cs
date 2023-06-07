using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Tools;
using Mini.Engine.Modelling.Paths;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackPieces
{
    public static TrackPiece Turn(Device device)
    {
        const float radius = 25.0f;
        const int points = 25;

        var builder = new PrimitiveMeshBuilder();
        var curve = new CircularArcCurve(0.0f, MathF.PI / 2.0f, radius);

        BuildRails(points, builder, curve);
        BuildTies(builder, curve);
        BuildBallast(points, builder, curve);

        var primitive = builder.Build(device, "Turn", out var bounds);

        return new TrackPiece(curve, primitive, bounds);
    }

    private static void BuildRails(int points, PrimitiveMeshBuilder builder, ICurve curve)
    {
        var partBuilder = builder.StartPart();

        var crossSection = CrossSections.RailCrossSection();
        Extruder.Extrude(partBuilder, crossSection, curve.OffsetLeft(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetLeft(SINGLE_RAIL_OFFSET), crossSection);

        Extruder.Extrude(partBuilder, crossSection, curve.OffsetRight(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetRight(SINGLE_RAIL_OFFSET), crossSection);
        partBuilder.Complete(RAIL_COLOR);
    }

    private static void BuildTies(PrimitiveMeshBuilder builder, ICurve curve)
    {
        var partBuilder = builder.StartPart();

        var front = CrossSections.TieCrossSectionFront();
        var back = CrossSections.TieCrossSectionBack();

        Joiner.Join(partBuilder, front, back);

        Filler.Fill(partBuilder, front);        
        Filler.Fill(partBuilder, back.Reverse());  // To avoid culling

        var transforms = Walker.WalkSpacedOut(curve, RAIL_TIE_SPACING, Vector3.UnitY);

        partBuilder.Layout(transforms);

        partBuilder.Complete(RAIL_TIE_COLOR);
    }

    private static void BuildBallast(int points, PrimitiveMeshBuilder builder, CircularArcCurve curve)
    {
        var partBuilder = builder.StartPart();

        var crossSection = CrossSections.BallastCrossSection();

        Extruder.Extrude(partBuilder, crossSection, curve, points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve, crossSection);

        
        partBuilder.Complete(BALLAST_COLOR);
    }

}
