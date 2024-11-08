﻿using System.Diagnostics;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Joiner
{
    public static void Join(IPrimitiveMeshPartBuilder builder, Path3D front, Path3D back)
    {
        Debug.Assert(front.Length == back.Length);
        Debug.Assert(front.IsClosed == back.IsClosed);

        for (var i = 0; i < front.Steps; i++)
        {
            builder.AddQuad(front[i], back[i], back[i + 1], front[i + 1]);            
        }        
    }
}
