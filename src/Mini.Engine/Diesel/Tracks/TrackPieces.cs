using System.Numerics;
using LibGame.Geometry;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;
using Mini.Engine.Modelling.Tools;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackPieces
{
    public static ILifetime<PrimitiveMesh> FromCurve(Device device, ICurve curve, int points, string name)
    {
        var builder = new PrimitiveMeshBuilder();

        BuildRails(points, builder, curve);
        BuildTies(builder, curve);
        BuildBallast(points, builder, curve);

        return builder.Build(device, name, out var bounds);
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

        var backFill = back.Reverse(); // To avoid culling

        Filler.Fill(partBuilder, front, Triangles.GetNormal(front[0], front[1], front[2]));
        Filler.Fill(partBuilder, backFill, Triangles.GetNormal(backFill[0], backFill[1], backFill[2]));  

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
