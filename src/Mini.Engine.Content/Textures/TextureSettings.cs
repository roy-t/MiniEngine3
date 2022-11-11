using SuperCompressed;

namespace Mini.Engine.Content.Textures;

public sealed record TextureSettings(Mode Mode, bool ShouldMipMap)
{
    public static readonly TextureSettings Default = new(Mode.SRgb, true);
    public static readonly TextureSettings NormalMaps = new(Mode.Normalized, true);
    public static readonly TextureSettings RenderData = new(Mode.Linear, true);
}
