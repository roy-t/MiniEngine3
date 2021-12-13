namespace Mini.Engine.Content.Materials;

internal record class MaterialData(string Id, int Index, string Albedo, string Metalicness, string Normal, string Roughness, string AmbientOcclusion)
    : IContentData;
