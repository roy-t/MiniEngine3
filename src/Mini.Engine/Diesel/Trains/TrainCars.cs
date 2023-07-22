using System.Diagnostics;
using System.Numerics;
using LibGame.Geometry;
using Mini.Engine.Content;
using Mini.Engine.Content.Models;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;
using Mini.Engine.Modelling.Tools;
using Vortice.Mathematics;
using static Mini.Engine.Diesel.Tracks.TrackParameters;
using static Mini.Engine.Diesel.Trains.TrainParameters;
using MeshPart = Mini.Engine.Content.Shaders.Generated.Primitive.MeshPart;

namespace Mini.Engine.Diesel.Trains;
public static class TrainCars
{

    // obj files should be exported from Blender with the following setings:
    // Scale: 1.0 (assuming you worked in Blender with 1 unit is 1 meter)
    // Forward Axis: -Z
    // Up Axis: Y
    // Apply Modifiers: true    
    // Normals: true
    // Triangulated Mesh: true
    // Object Groups: true
    // The object does not need to be centered in the obj file, that is done for you in the code below.
    // This makes it easier to export selected items from a larger Blender workspace.

    public static ILifetime<PrimitiveMesh> BuildBogie(Device device, ContentManager content, string name)
    {
        var modelReference = content.LoadModelData(@"Diesel\bogie.obj", ModelSettings.Default);
        var model = device.Resources.Get(modelReference);

        var parts = CreateMeshParts(model.Vertices.Length, BOGIE_COLOR, BOGIE_METALICNESS, BOGIE_ROUGHNESS);
        var yOffset = SINGLE_RAIL_HEIGTH + BALLAST_HEIGHT_TOP - 0.06f;
        var vertices = CreateVertices(model.Bounds, model.Vertices, new Vector3(0.0f, yOffset, 0.0f));

        return device.Resources.Add(new PrimitiveMesh(device, vertices, model.Indices.Span, parts, model.Bounds, name));
    }

    public static ILifetime<PrimitiveMesh> BuildFlatCar(Device device, ContentManager content, string name, in BoundingBox bogieBounds)
    {
        var modelReference = content.LoadModelData(@"Diesel\flat_car.obj", ModelSettings.Default);
        var model = device.Resources.Get(modelReference);

        var parts = CreateMeshParts(model.Vertices.Length, BOGIE_COLOR, BOGIE_METALICNESS, BOGIE_ROUGHNESS);
        var yOffset = SINGLE_RAIL_HEIGTH + BALLAST_HEIGHT_TOP + bogieBounds.Height - 0.25f;
        var vertices = CreateVertices(model.Bounds, model.Vertices, new Vector3(0.0f, yOffset, 0.0f));

        return device.Resources.Add(new PrimitiveMesh(device, vertices, model.Indices.Span, parts, model.Bounds, name));
    }

    private static ReadOnlySpan<MeshPart> CreateMeshParts(int vertexCount, Color4 color, float metalicness, float roughness)
    {
        return new MeshPart[]
        {
            new MeshPart
            {
                Offset = 0,
                Length = (uint)vertexCount,
                Albedo = color,
                Metalicness = metalicness,
                Roughness = roughness,
            }
        };
    }

    private static ReadOnlySpan<PrimitiveVertex> CreateVertices(BoundingBox bounds, ReadOnlyMemory<ModelVertex> vertices, Vector3 offset = default)
    {
        // Place the model centered on the floor plane
        var center = new Vector3(-bounds.Center.X, -bounds.Min.Y, -bounds.Center.Z);
        var transform = Matrix4x4.CreateTranslation(center + offset);

        var output = new PrimitiveVertex[vertices.Length];
        var span = vertices.Span;
        for (var i = 0; i < span.Length; i++)
        {
            var vertex = span[i];
            var position = Vector3.Transform(vertex.Position, transform);
            output[i] = new PrimitiveVertex(position, vertex.Normal);
        }

        return output;
    }

    public static ILifetime<PrimitiveMesh> BuildBogie(Device device, string name)
    {
        var builder = new PrimitiveMeshBuilder();
        BuildBogie(builder);

        return builder.Build(device, name, out var _);
    }

    public static ILifetime<PrimitiveMesh> BuildFlatCar(Device device, string name)
    {
        var builder = new PrimitiveMeshBuilder();
        BuildFlatBed(builder);

        return builder.Build(device, name, out var _);
    }

    private static void BuildFlatBed(PrimitiveMeshBuilder builder)
    {
        var partBuilder = builder.StartPart();

        var crossSection = CrossSections.Bed();

        var halfCarWidth = FLAT_CAR_WIDTH * 0.5f;
        var curve = new StraightCurve(new Vector3(0, 0, halfCarWidth), new Vector3(0.0f, 0.0f, -1.0f), FLAT_CAR_WIDTH);
        Extruder.Extrude(partBuilder, crossSection, curve, 2, Vector3.UnitY);

        var fillPath = crossSection.ToPath3D(halfCarWidth);
        var fillPath2 = crossSection.ToPath3D(-halfCarWidth).Reverse();

        Filler.Fill(partBuilder, fillPath, Vector3.UnitZ);
        Filler.Fill(partBuilder, fillPath2, -Vector3.UnitZ);

        partBuilder.Transform(FLAT_CAR_TRANSFORM);
        partBuilder.Complete(BOGIE_COLOR, BOGIE_METALICNESS, BOGIE_ROUGHNESS);
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
            var path = crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition(0)));
            var normal = Triangles.GetNormal(path[0], path[1], path[2]);
            Filler.Fill(partBuilder, path, normal);
        }
        else
        {
            var path = crossSection.ToPath3D().Transform(Matrix4x4.CreateTranslation(curve.GetPosition(1))).Reverse();
            var normal = Triangles.GetNormal(path[0], path[1], path[2]);
            Filler.Fill(partBuilder, path, normal);
        }
    }
}

