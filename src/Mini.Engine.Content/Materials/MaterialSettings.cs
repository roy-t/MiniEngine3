using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.Materials;
public sealed record MaterialSettings(TextureSettings AlbedoFormat, TextureSettings MetalicnessFormat, TextureSettings NormalFormat, TextureSettings RoughnessFormat, TextureSettings AmbientOcclusionFormat)
{
    public static readonly MaterialSettings Default = new(TextureSettings.Default, TextureSettings.RenderData, TextureSettings.NormalMaps, TextureSettings.RenderData, TextureSettings.RenderData);
}
