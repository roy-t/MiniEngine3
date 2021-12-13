using Mini.Engine.Content.Materials;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Models;

internal sealed record ModelData(string Id, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, MaterialData[] Materials)
: IContentData;
