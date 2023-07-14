using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;
using Mini.Engine.Modelling.Tools;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackPieces
{    
    public static TrackPiece Straight(Device device, Entity entity)
    {
        var curve = new StraightCurve(new Vector3(0.0f, 0.0f, STRAIGHT_LENGTH * 0.5f), new Vector3(0.0f, 0.0f, -1.0f), STRAIGHT_LENGTH);
        return FromCurve(device, entity, curve, 2, nameof(Straight));
    }

    public static TrackPiece FromCurve(Device device, Entity entity, ICurve curve, int points, string name)
    {
        var builder = new PrimitiveMeshBuilder();

        BuildRails(points, builder, curve);
        BuildTies(builder, curve);
        BuildBallast(points, builder, curve);

        var primitive = builder.Build(device, name, out var bounds);

        return new TrackPiece(entity, name, curve, primitive, bounds);
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
