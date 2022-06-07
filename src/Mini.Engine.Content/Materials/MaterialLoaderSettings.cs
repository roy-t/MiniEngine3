using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.Materials;
public sealed record MaterialLoaderSettings(TextureLoaderSettings AlbedoFormat, TextureLoaderSettings MetalicnessFormat, TextureLoaderSettings NormalFormat, TextureLoaderSettings RoughnessFormat, TextureLoaderSettings AmbientOcclusionFormat) : ILoaderSettings
{
    public static MaterialLoaderSettings Default = new(TextureLoaderSettings.Default, TextureLoaderSettings.RenderData, TextureLoaderSettings.NormalMaps, TextureLoaderSettings.RenderData, TextureLoaderSettings.RenderData);
}
