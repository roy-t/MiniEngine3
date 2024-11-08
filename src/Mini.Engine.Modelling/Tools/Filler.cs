﻿using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Filler
{
    /// <summary>
    /// Triangulates and fills the given path, assumes all vertices in path are in the same plane
    /// </summary>
    public static void Fill(IPrimitiveMeshPartBuilder builder, Path3D path, Vector3 normal)
    {
        Debug.Assert(path.Length >= 3);

        var indices = EarClipping.Triangulate(path.Positions, normal);        

        var startIndex = int.MaxValue;
        for (var i = 0; i < path.Positions.Length; i++)
        {
            var index = builder.AddVertex(path.Positions[i], normal);
            startIndex = Math.Min(startIndex, index);
        }

        for (var i = 0; i < indices.Length; i++)
        {
            builder.AddIndex(indices[i] + startIndex);
        }
    }
}
