using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;
using Mini.Engine.Modelling.Tools;

using static Mini.Engine.Diesel.Trains.TrainParameters;

namespace Mini.Engine.Diesel.Trains;
public static class TrainCars
{
    // TODO: a lof of invisible geometry is generated like bottoms and behinds

    public static TrainCar Flatcar(Device device)
    {
        var builder = new PrimitiveMeshBuilder();
        BuildWheel(builder);

        var primitive = builder.Build(device, "Flatcar", out var bounds);


        return new TrainCar(primitive, bounds, Transform.Identity);
    }

    private static void BuildWheel(PrimitiveMeshBuilder builder)
    {
        var partBuilder = builder.StartPart();
      
        BuildSide(partBuilder, 1.0f);
        BuildSide(partBuilder, -1.0f);
        
        partBuilder.Transform(WHEEL_TRANSFORM);
        partBuilder.Complete(BOGIE_COLOR, BOGIE_METALICNESS, BOGIE_ROUGHNESS);
    }

    private static void BuildSide(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float direction)
    {
        var plateOffset = direction * (TrackParameters.SINGLE_RAIL_OFFSET + OUTER_WHEEL_THICKNESS);

        BuildWheelAndAxle(partBuilder, direction, WHEEL_SPACING * 0.5f);
        BuildWheelAndAxle(partBuilder, direction, WHEEL_SPACING * -0.5f);
        BuildPlate(partBuilder, OUTER_WHEEL_THICKNESS, new Vector2(0.0f, plateOffset));
    }

    private static void BuildWheelAndAxle(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float direction, float forward)
    {        
        var center = new Vector2(forward, direction * TrackParameters.SINGLE_RAIL_OFFSET);
        var inner = center + new Vector2(0.0f, -direction * ((OUTER_WHEEL_THICKNESS * 0.5f) + (INNER_WHEEL_THICKNESS * 0.5f)));        
        var outer = center + new Vector2(0.0f, +direction * ((OUTER_WHEEL_THICKNESS * 0.5f) + (INNER_WHEEL_THICKNESS * 0.5f) + OUTER_WHEEL_THICKNESS));

        BuildWheel(partBuilder, INNER_WHEEL_RADIUS, INNER_WHEEL_THICKNESS, inner);
        BuildWheel(partBuilder, OUTER_WHEEL_RADIUS, OUTER_WHEEL_THICKNESS, center);        
        BuildWheel(partBuilder, AXLE_RADIUS, INNER_WHEEL_THICKNESS, outer);
    }

    private static void BuildWheel(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float radius, float length, Vector2 offset)
    {
        var crossSection = CrossSections.Circle(radius, WHEEL_VERTICES);
        JoinEnds(partBuilder, crossSection, offset, length);
    }

    private static void BuildPlate(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float length, Vector2 offset)
    {
        var crossSection = CrossSections.Plate();
        JoinEnds(partBuilder, crossSection, offset, length);
    }

    private static void JoinEnds(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, Path2D crossSection, Vector2 offset, float length)
    {
        var curve = new StraightCurve(new Vector2(offset.X, offset.Y - (length * 0.5f)), Vector2.UnitY, length);

        Extruder.Extrude(partBuilder, crossSection, curve, 2, Vector3.UnitY);

        Filler.Fill(partBuilder, crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition3D(0))));
        Filler.Fill(partBuilder, crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition3D(1))).Reverse());
    }
}

