using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Models;

internal sealed record ModelData(ContentId Id, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, Material[] Materials)
: IContentData;
