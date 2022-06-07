namespace Mini.Engine.Content.Textures;

public sealed record TextureLoaderSettings(bool IsSRgb, bool IsNormalized, bool ShouldMipMap) : ILoaderSettings
{
    public static TextureLoaderSettings Default = new(true, false, true);
    public static TextureLoaderSettings NormalMaps = new(false, true, true);
    public static TextureLoaderSettings RenderData = new(false, false, true);
}
