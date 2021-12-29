using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Content.Models;

internal sealed record ModelData(ContentId Id, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, IMaterial[] Materials)
: IContentData;
