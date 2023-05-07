using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling;

public record struct Quad(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, Vector3 D)
{
    public Quad CreateTransformed(in Transform transform)
    {
        var matrix = transform.GetMatrix();
        var normal = Vector3.TransformNormal(this.Normal, matrix);
        var a = Vector3.Transform(this.A, matrix);
        var b = Vector3.Transform(this.B, matrix);
        var c = Vector3.Transform(this.C, matrix);
        var d = Vector3.Transform(this.D, matrix);

        return new Quad(normal, a, b, c, d);
    }

    public static Quad SingleFromPath(Path2D path, Vector3 normal, int i0 = 0, int i1 = 1, int i2 = 2, int i3 = 3)
    {
        var topRight = path[i0].ToVector3();
        var bottomRight = path[i1].ToVector3();
        var bottomLeft = path[i2].ToVector3();
        var topLeft = path[i3].ToVector3();

        return new Quad(normal, topRight, bottomRight, bottomLeft, topLeft);
    }

    public static Quad[] MultipleFromPath(Path2D path, Vector3 normal, params int[] indices)
    {
        Debug.Assert(indices.Length >= 4 && indices.Length % 4 == 0);

        var quads = new Quad[indices.Length / 4];
        for(var i = 0; i < quads.Length; i++)
        {
            quads[i] = SingleFromPath(path, normal, indices[(i * 4) + 0], indices[(i * 4) + 1], indices[(i * 4) + 2], indices[(i * 4) + 3]);
        }

        return quads;
    }
}
