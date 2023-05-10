using System.Diagnostics;

namespace Mini.Engine.Modelling.Tools;
public static class Joiner
{
    public static Quad[] Join(Path3D front, Path3D back)
    {
        Debug.Assert(front.Length == back.Length);
        Debug.Assert(front.IsClosed == back.IsClosed);

        var length = front.IsClosed ? front.Length : (front.Length - 1);
        var quads = new Quad[length];

        for (var i = 0; i < length; i++)
        {
            quads[i] = new Quad(back[i], back[i + 1], front[i + 1], front[i]);
        }

        return quads;
    }
}
