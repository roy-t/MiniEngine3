using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX.Resources.Models;

namespace Mini.Engine.Content.Models;
internal static class SerializationExtensions
{
    public static void Write(this ContentWriter writer, ModelSettings modelSettings)
    {
        writer.Write(modelSettings.MaterialSettings);
    }

    public static ModelSettings ReadModelSettings(this ContentReader reader)
    {
        var material = reader.ReadMaterialSettings();
        return new ModelSettings(material);
    }

    public static void Write(this ContentWriter writer, ModelVertex vertex)
    {
        writer.Write(vertex.Position);
        writer.Write(vertex.Texcoord);
        writer.Write(vertex.Normal);
    }

    public static ModelVertex ReadModelVertex(this ContentReader reader)
    {
        return new ModelVertex
        (
            reader.ReadVector3(),
            reader.ReadVector2(),
            reader.ReadVector3()
        );
    }

    public static void Write(this ContentWriter writer, ModelVertex[] vertices)
    {
        writer.Writer.Write(vertices.Length);
        foreach (var vertex in vertices)
        {
            writer.Write(vertex);
        }
    }

    public static ReadOnlyMemory<ModelVertex> ReadModelVertices(this ContentReader reader)
    {
        var vertices = new ModelVertex[reader.Reader.ReadInt32()];
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = reader.ReadModelVertex();
        }

        return vertices;
    }

    public static void Write(this ContentWriter writer, ModelPart primitive)
    {
        writer.Writer.Write(primitive.Name);
        writer.Write(primitive.Bounds);
        writer.Writer.Write(primitive.MaterialIndex);
        writer.Writer.Write(primitive.IndexOffset);
        writer.Writer.Write(primitive.IndexCount);
    }

    public static ModelPart ReadPrimitive(this ContentReader reader)
    {
        return new ModelPart
        (
            reader.Reader.ReadString(),
            reader.ReadBoundingBox(),
            reader.Reader.ReadInt32(),
            reader.Reader.ReadInt32(),
            reader.Reader.ReadInt32()
        );
    }

    public static void Write(this ContentWriter writer, ModelPart[] primitives)
    {
        writer.Writer.Write(primitives.Length);
        foreach (var primitive in primitives)
        {
            writer.Write(primitive);
        }
    }

    public static ReadOnlyMemory<ModelPart> ReadPrimitives(this ContentReader reader)
    {
        var primitives = new ModelPart[reader.Reader.ReadInt32()];
        for (var i = 0; i < primitives.Length; i++)
        {
            primitives[i] = reader.ReadPrimitive();
        }

        return primitives;
    }

    public static void Write(this ContentWriter writer, int[] indices)
    {
        writer.Writer.Write(indices.Length);
        foreach (var index in indices)
        {
            writer.Writer.Write(index);
        }
    }

    public static ReadOnlyMemory<int> ReadIndices(this ContentReader reader)
    {
        var indices = new int[reader.Reader.ReadInt32()];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = reader.Reader.ReadInt32();
        }

        return indices;
    }
}
