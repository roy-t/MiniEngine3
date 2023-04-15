using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Serialization;
public static class SerializationExtensions
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

    public static void Write(this ContentWriter writer, Vector4 vector)
    {
        writer.Writer.Write(vector.X);
        writer.Writer.Write(vector.Y);
        writer.Writer.Write(vector.Z);
        writer.Writer.Write(vector.W);
    }

    public static Vector4 ReadVector4(this ContentReader reader)
    {
        return new Vector4
        {
            X = reader.Reader.ReadSingle(),
            Y = reader.Reader.ReadSingle(),
            Z = reader.Reader.ReadSingle(),
            W = reader.Reader.ReadSingle()
        };
    }

    public static void Write(this ContentWriter writer, BoundingBox boundingBox)
    {
        writer.Write(boundingBox.Min);
        writer.Write(boundingBox.Max);
    }

    public static BoundingBox ReadBoundingBox(this ContentReader reader)
    {
        return new BoundingBox(reader.ReadVector3(), reader.ReadVector3());
    }

    public static void Write(this ContentWriter writer, ContentId id)
    {
        writer.Writer.Write(id.Path);
        writer.Writer.Write(id.Key);
    }

    public static ContentId ReadContentId(this ContentReader reader)
    {
        var path = reader.Reader.ReadString();
        var key = reader.Reader.ReadString();

        return new ContentId(path, key);
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
