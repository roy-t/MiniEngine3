using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Tools;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackPieces
{
    public static TrackPiece Turn(Device device)
    {
        const float radius = 25.0f;
        const int points = 50;

        var builder = new PrimitiveMeshBuilder();
        var curve = new CircularArcCurve(0.0f, MathF.PI / 2.0f, radius);
        var crossSection = CrossSections.RailCrossSection();

        var partBuilder = builder.StartPart();
        Extruder.Extrude(partBuilder, crossSection, curve.OffsetLeft(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetLeft(SINGLE_RAIL_OFFSET), crossSection);

        Extruder.Extrude(partBuilder, crossSection, curve.OffsetRight(SINGLE_RAIL_OFFSET), points, Vector3.UnitY);
        Capper.Cap(partBuilder, curve.OffsetRight(SINGLE_RAIL_OFFSET), crossSection);        
        partBuilder.Complete(RAIL_COLOR);

        var primitive = builder.Build(device, "Turn", out var bounds);

        return new TrackPiece(curve, primitive, bounds);
    }
}
