using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling;

public record struct Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
{
    public Vector3 GetNormal()
    {
        return QuadUtilities.GetNormal(this.A, this.B, this.C, this.D);
    }

    public Quad CreateTransformed(in Transform transform)
    {
        var matrix = transform.GetMatrix();
        var a = Vector3.Transform(this.A, matrix);
        var b = Vector3.Transform(this.B, matrix);
        var c = Vector3.Transform(this.C, matrix);
        var d = Vector3.Transform(this.D, matrix);

        return new Quad(a, b, c, d);
    }

    public static Quad SingleFromPath(Path3D path, int i0 = 0, int i1 = 1, int i2 = 2, int i3 = 3)
    {
        var topRight = path[i0];
        var bottomRight = path[i1];
        var bottomLeft = path[i2];
        var topLeft = path[i3];

        return new Quad(topRight, bottomRight, bottomLeft, topLeft);
    }

    public static Quad SingleFromPath(Path2D path, int i0 = 0, int i1 = 1, int i2 = 2, int i3 = 3)
    {
        return SingleFromPath(path.ToPath3D(), i0, i1, i2, i3);
    }

    public static Quad[] MultipleFromPath(Path3D path, params int[] indices)
    {
        Debug.Assert(indices.Length >= 4 && indices.Length % 4 == 0);

        var quads = new Quad[indices.Length / 4];
        for (var i = 0; i < quads.Length; i++)
        {
            quads[i] = SingleFromPath(path, indices[(i * 4) + 0], indices[(i * 4) + 1], indices[(i * 4) + 2], indices[(i * 4) + 3]);
        }

        return quads;
    }

    public static Quad[] MultipleFromPath(Path2D path, params int[] indices)
    {
        return MultipleFromPath(path.ToPath3D(), indices);
    }
}
