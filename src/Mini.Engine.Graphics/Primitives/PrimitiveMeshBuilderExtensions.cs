﻿using System.Numerics;
using LibGame.Geometry;

namespace Mini.Engine.Graphics.Primitives;
public static class PrimitiveMeshBuilderExtensions
{
    public static void AddQuad(this IPrimitiveMeshPartBuilder builder, Vector3 tr, Vector3 br, Vector3 bl, Vector3 tl)
    {
        var normal = Quads.GetNormal(tr, br, bl, tl);
        var iTr = builder.AddVertex(new PrimitiveVertex(tr, normal));
        var iBr = builder.AddVertex(new PrimitiveVertex(br, normal));
        var iBl = builder.AddVertex(new PrimitiveVertex(bl, normal));
        var iTl = builder.AddVertex(new PrimitiveVertex(tl, normal));

        builder.AddIndex(iTr);
        builder.AddIndex(iBr);
        builder.AddIndex(iTl);

        builder.AddIndex(iBr);
        builder.AddIndex(iBl);
        builder.AddIndex(iTl);
    }
}
