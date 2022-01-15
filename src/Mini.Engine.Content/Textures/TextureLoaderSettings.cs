using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed record TextureLoaderSettings(Format? PreferredFormat) : ILoaderSettings
{
    public static TextureLoaderSettings Default = new((Format?)null);
}
