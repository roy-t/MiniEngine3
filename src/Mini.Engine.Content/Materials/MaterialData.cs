namespace Mini.Engine.Content.Materials;

internal record class MaterialData(ContentId Id, int Index, string Albedo, string Metalicness, string Normal, string Roughness, string AmbientOcclusion)
    : IContentData;
