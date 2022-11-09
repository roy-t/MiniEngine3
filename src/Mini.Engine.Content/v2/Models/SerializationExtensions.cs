using System.Net.Http;
using System.Numerics;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX.Resources.Models;
using Vortice.Mathematics;

namespace Mini.Engine.Content.v2.Models;
internal static class SerializationExtensions
{
    public static void Write(this ContentWriter writer, Vector2 vector)
    {
        writer.Writer.Write(vector.X);
        writer.Writer.Write(vector.Y);
    }

    public static Vector2 ReadVector2(this ContentReader reader)
    {
        return new Vector2
        {
            X = reader.Reader.ReadSingle(),
            Y = reader.Reader.ReadSingle()
        };
    }

    public static void Write(this ContentWriter writer, Vector3 vector)
    {
        writer.Writer.Write(vector.X);
        writer.Writer.Write(vector.Y);
        writer.Writer.Write(vector.Z);
    }

    public static Vector3 ReadVector3(this ContentReader reader)
    {
        return new Vector3
        {
            X = reader.Reader.ReadSingle(),
            Y = reader.Reader.ReadSingle(),
            Z = reader.Reader.ReadSingle()
        };
    }

    public static void Write(this ContentWriter writer, BoundingBox boundingBox)
    {
        writer.Write(boundingBox.Min);
        writer.Write(boundingBox.Max);
    }

    public static BoundingBox ReadBoundingBox(this ContentReader reader)
    {
        return new BoundingBox
        {
            Min = reader.ReadVector3(),
            Max = reader.ReadVector3()
        };
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
            vertices[i] = ReadModelVertex(reader);
        }

        return vertices;
    }

    public static void Write(this ContentWriter writer, Primitive primitive)
    {
        writer.Writer.Write(primitive.Name);
        writer.Write(primitive.Bounds);
        writer.Writer.Write(primitive.MaterialIndex);
        writer.Writer.Write(primitive.IndexOffset);
        writer.Writer.Write(primitive.IndexCount);
    }

    public static Primitive ReadPrimitive(this ContentReader reader)
    {
        return new Primitive
        (
            reader.Reader.ReadString(),
            reader.ReadBoundingBox(),
            reader.Reader.ReadInt32(),
            reader.Reader.ReadInt32(),
            reader.Reader.ReadInt32()
        );
    }

    public static void Write(this ContentWriter writer, Primitive[] primitives)
    {
        writer.Writer.Write(primitives.Length);
        foreach (var primitive in primitives)
        {
            writer.Write(primitive);
        }
    }

    public static IReadOnlyList<Primitive> ReadPrimitives(this ContentReader reader)
    {
        var primitives = new Primitive[reader.Reader.ReadInt32()];
        for (var i = 0; i < primitives.Length; i++)
        {
            primitives[i] = ReadPrimitive(reader);
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

    public static void Write(this ContentWriter writer, ContentId[] contentIds)
    {
        writer.Writer.Write(contentIds.Length);
        foreach (var contentId in contentIds)
        {
            writer.Write(contentId);
}
}

    public static IReadOnlyList<ContentId> ReadContentIds(this ContentReader reader)
    {
        var contentIds = new ContentId[reader.Reader.ReadInt32()];
        for (var i = 0; i < contentIds.Length; i++)
        {
            contentIds[i] = reader.ReadContentId();
        }

        return contentIds;
    }
}
