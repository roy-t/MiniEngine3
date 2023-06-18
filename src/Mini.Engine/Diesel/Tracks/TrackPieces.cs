using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Tools;
using Mini.Engine.Modelling.Paths;
using static Mini.Engine.Diesel.Tracks.TrackParameters;
using Mini.Engine.ECS;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackPieces
{
    public static TrackPiece Turn(Device device, Entity entity)
    {
        var builder = new PrimitiveMeshBuilder();
        var curve = new CircularArcCurve(0.0f, MathF.PI / 2.0f, TURN_RADIUS);
        
        BuildRails(TURN_VERTICES, builder, curve);
        BuildTies(builder, curve);
        BuildBallast(TURN_VERTICES, builder, curve);

        var primitive = builder.Build(device, "Turn", out var bounds);

        return new TrackPiece(entity, nameof(Turn), curve, primitive, bounds);
    }

    public static TrackPiece Straight(Device device, Entity entity)
    {
        const int points = 2;
        var builder = new PrimitiveMeshBuilder();
        var curve = new StraightCurve(new Vector2(0, -STRAIGHT_LENGTH * 0.5f), Vector2.UnitY, STRAIGHT_LENGTH);

        BuildRails(points, builder, curve);
        BuildTies(builder, curve);
        BuildBallast(points, builder, curve);

        var primitive = builder.Build(device, "Turn", out var bounds);

        return new TrackPiece(entity, nameof(Straight), curve, primitive, bounds);
    }

    private static void BuildRails(int points, PrimitiveMeshBuilder builder, ICurve curve)
    {
        var partBuilder = builder.StartPart();

        var crossSection = CrossSections.RailCrossSection();
        Extruder.ExtrudeSmooth(partBuilder, crossSection, curve.OffsetLeft(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetLeft(SINGLE_RAIL_OFFSET), crossSection);

        Extruder.ExtrudeSmooth(partBuilder, crossSection, curve.OffsetRight(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetRight(SINGLE_RAIL_OFFSET), crossSection);
        partBuilder.Complete(RAIL_COLOR, RAIL_METALICNESS, RAIL_ROUGHNESS);
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

        partBuilder.Complete(RAIL_TIE_COLOR, RAIL_TIE_METALICNESS, RAIL_TIE_ROUGHNESS);
    }

    private static void BuildBallast(int points, PrimitiveMeshBuilder builder, ICurve curve)
    {
        var partBuilder = builder.StartPart();

        var crossSection = CrossSections.BallastCrossSection();

        Extruder.ExtrudeSmooth(partBuilder, crossSection, curve, points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve, crossSection);

        
        partBuilder.Complete(BALLAST_COLOR, BALLAST_METALICNESS, BALLAST_ROUGHNESS);
    }

}
