using System.Diagnostics;

namespace Mini.Engine.Modelling.Tools;
public static class Joiner
{
    public static Quad[] Join(Path3D front, Path3D back)
    {
        Debug.Assert(front.Length == back.Length);
        Debug.Assert(front.IsClosed == back.IsClosed);
        
        var quads = new Quad[front.Steps];

        for (var i = 0; i < front.Steps; i++)
        {
            quads[i] = new Quad(front[i], back[i], back[i + 1], front[i + 1]);
        }

        return quads;
    }
}
