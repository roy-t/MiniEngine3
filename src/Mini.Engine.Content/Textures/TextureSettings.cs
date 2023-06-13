using SuperCompressed;

namespace Mini.Engine.Content.Textures;

public sealed record TextureSettings(Mode Mode, bool ShouldMipMap, bool ForceUncompressed)
{
    public static readonly TextureSettings Default = new(Mode.SRgb, true, false);
    public static readonly TextureSettings NormalMaps = new(Mode.Normalized, true, false);
    public static readonly TextureSettings RenderData = new(Mode.Linear, true, false);
}
