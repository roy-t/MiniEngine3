using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX.Resources.Models;

namespace Mini.Engine.Content.Models.Wavefront;

internal sealed class ModelVertexComparer : IEqualityComparer<ModelVertex>
{
    public bool Equals(ModelVertex x, ModelVertex y)
    {
        return x.Normal.Equals(y.Normal) && x.Position.Equals(y.Position) && x.Texcoord.Equals(y.Texcoord);
    }

    public int GetHashCode([DisallowNull] ModelVertex obj)
    {
        return HashCode.Combine(obj.Normal, obj.Position, obj.Texcoord);
    }
}