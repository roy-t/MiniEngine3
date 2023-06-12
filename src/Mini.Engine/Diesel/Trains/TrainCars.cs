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
        
        BuildWheel(partBuilder, INNER_WHEEL_RADIUS, new Vector2(0, TrackParameters.SINGLE_RAIL_OFFSET - WHEEL_THICKNESS));
        BuildWheel(partBuilder, OUTER_WHEEL_RADIUS, new Vector2(0, TrackParameters.SINGLE_RAIL_OFFSET));

        BuildWheel(partBuilder, INNER_WHEEL_RADIUS, -new Vector2(0, TrackParameters.SINGLE_RAIL_OFFSET - WHEEL_THICKNESS));
        BuildWheel(partBuilder, OUTER_WHEEL_RADIUS, -new Vector2(0, TrackParameters.SINGLE_RAIL_OFFSET));

        //curve = new StraightCurve(new Vector2(2.0f, 0), Vector2.UnitY, WHEEL_THICKNESS);
        //innerWheel = CrossSections.OuterWheel();
        //BuildWheel(partBuilder, innerWheel, curve);

        partBuilder.Transform(LEFT_WHEEL_TRANSFORM);

        partBuilder.Complete(BOGIE_COLOR, BOGIE_METALICNESS, BOGIE_ROUGHNESS);
    }

    private static void BuildWheel(PrimitiveMeshBuilder.PrimitiveMeshPartBuilder partBuilder, float radius, Vector2 offset)
    {
        var curve = new StraightCurve(new Vector2(offset.X, offset.Y - (WHEEL_THICKNESS * 0.5f)) , Vector2.UnitY, WHEEL_THICKNESS);
        var crossSection = CrossSections.Circle(radius, WHEEL_VERTICES);

        crossSection = new Path2D(crossSection.IsClosed, crossSection.Positions.Reverse().ToArray());

        Extruder.Extrude(partBuilder, crossSection, curve, 2, Vector3.UnitY);

        // TODO: make it possible to transform a path so that we can also include the X component of offset to add the 3rd and 4th wheel!
        Filler.Fill(partBuilder, crossSection.ToPath3D(curve.GetPosition3D(0).Z));
        Filler.Fill(partBuilder, crossSection.ToPath3D(curve.GetPosition3D(1).Z).Reverse());
    }
}

