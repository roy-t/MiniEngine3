using System.Numerics;

namespace Mini.Engine.Modelling;

public interface IFace
{
    public IReadOnlyList<int> Indices { get; }
    public Vector3 Normal { get; }
    public void Triangulate(List<TriangleFace> buffer);
}

public sealed class QuadFace : PolygonFace
{
    public QuadFace(Vector3 normal, int a, int b, int c, int d)
        : base(normal, a, b, c, d) { }

    public override void Triangulate(List<TriangleFace> buffer)
    {
        buffer.Add(new TriangleFace(this.Normal, this.Indices[0], this.Indices[1], this.Indices[2]));
        buffer.Add(new TriangleFace(this.Normal, this.Indices[2], this.Indices[3], this.Indices[0]));
    }
}

public sealed class TriangleFace : PolygonFace
{
    public TriangleFace(Vector3 normal, int a, int b, int c)
        : base(normal, a, b, c) { }

    public override void Triangulate(List<TriangleFace> buffer)
    {
        buffer.Add(this);
    }
}

public abstract class PolygonFace : IFace
{
    private readonly int[] IndexArray;

    public PolygonFace(Vector3 normal, params int[] indices)
    {
        this.Normal = normal;
        this.IndexArray = indices;
    }

    public IReadOnlyList<int> Indices => this.IndexArray;
    public Vector3 Normal { get; set; }

    public abstract void Triangulate(List<TriangleFace> buffer);
}

public sealed class Geometry
{
    public Geometry()
    {
        this.Vertices = new List<Vector3>();
        this.Faces = new List<IFace>();
    }

    public List<Vector3> Vertices { get; private set; }
    public List<IFace> Faces { get; private set; }

    public int AddVertex(Vector3 vertex)
    {
        var count = this.Vertices.Count;
        this.Vertices.Add(vertex);

        return count;
    }

    public void AddFace(IFace face)
    {
        this.Faces.Add(face);
    }

    public QuadFace AddFace(Vector3 normal, int a, int b, int c, int d)
    {
        var quad = new QuadFace(normal, a, b, c, d);
        this.AddFace(quad);

        return quad;
    }

    public TriangleFace AddFace(Vector3 normal, int a, int b, int c)
    {
        var triangle = new TriangleFace(normal, a, b, c);
        this.AddFace(triangle);

        return triangle;
    }
}
