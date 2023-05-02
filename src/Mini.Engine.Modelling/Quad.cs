using System.Numerics;
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
}
