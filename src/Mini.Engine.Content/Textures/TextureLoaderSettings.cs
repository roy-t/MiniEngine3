using SuperCompressed;

namespace Mini.Engine.Content.Textures;

public sealed record TextureLoaderSettings(Mode Mode, bool ShouldMipMap) : ILoaderSettings
{
    public static TextureLoaderSettings Default = new(Mode.SRgb, true);
    public static TextureLoaderSettings NormalMaps = new(Mode.Normalized, true);
    public static TextureLoaderSettings RenderData = new(Mode.Linear, true);
}
