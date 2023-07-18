using System.Numerics;
using Mini.Engine.Core.Lifetime;
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
    public static ILifetime<PrimitiveMesh> BuildBogie(Device device, string name)
    {
        var builder = new PrimitiveMeshBuilder();
        BuildBogie(builder);

        return builder.Build(device, name, out var bounds);
    }    

    private static void BuildBogie(PrimitiveMeshBuilder builder)
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
        BuildPlate(partBuilder, OUTER_WHEEL_THICKNESS, new Vector2(0.0f, plateOffset), direction);
    }

    private static void BuildWheelAndAxle(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float direction, float forward)
    {
        var center = new Vector2(forward, direction * TrackParameters.SINGLE_RAIL_OFFSET);
        var inner = center + new Vector2(0.0f, -direction * ((OUTER_WHEEL_THICKNESS * 0.5f) + (INNER_WHEEL_THICKNESS * 0.5f)));
        var outer = center + new Vector2(0.0f, +direction * ((OUTER_WHEEL_THICKNESS * 0.5f) + (INNER_WHEEL_THICKNESS * 0.5f) + OUTER_WHEEL_THICKNESS));

        BuildWheel(partBuilder, INNER_WHEEL_RADIUS, INNER_WHEEL_THICKNESS, inner, direction);
        BuildWheel(partBuilder, OUTER_WHEEL_RADIUS, OUTER_WHEEL_THICKNESS, center, direction);
        BuildWheel(partBuilder, AXLE_RADIUS, INNER_WHEEL_THICKNESS, outer, direction);
    }

    private static void BuildWheel(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float radius, float length, Vector2 offset, float direction)
    {
        var crossSection = CrossSections.Wheel(radius, WHEEL_VERTICES);
        ExtrudeAndCapOuterEnd(partBuilder, crossSection, offset, length, direction);
    }

    private static void BuildPlate(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float length, Vector2 offset, float direction)
    {
        var crossSection = CrossSections.Plate();
        ExtrudeAndCapOuterEnd(partBuilder, crossSection, offset, length, direction);
    }

    private static void ExtrudeAndCapOuterEnd(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, Path2D crossSection, Vector2 offset, float length, float direction)
    {
        var curve = new StraightCurve(new Vector3(offset.X, 0.0f, -(offset.Y - (length * 0.5f))), new Vector3(0, 0, -1), length);

        Extruder.Extrude(partBuilder, crossSection, curve, 2, Vector3.UnitY);

        if (direction < 0)
        {
            Filler.Fill(partBuilder, crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition(0))));
        }
        else
        {
            Filler.Fill(partBuilder, crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition(1))).Reverse());
        }
    }
}

