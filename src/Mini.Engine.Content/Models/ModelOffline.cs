using Mini.Engine.DirectX.Resources.Models;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models;

internal sealed record ModelOffline(BoundingBox Bounds, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, ContentId[] Materials);
