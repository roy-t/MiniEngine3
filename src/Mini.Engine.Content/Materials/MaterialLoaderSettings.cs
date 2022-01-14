using Vortice.DXGI;

namespace Mini.Engine.Content.Materials;
internal sealed record MaterialLoaderSettings(Format AlbedoFormat, Format MetalicnessFormat, Format NormalFormat, Format RoughnessFormat, Format AmbientOcclusionFormat) : ILoaderSettings
{
    public static MaterialLoaderSettings Default = new(Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm);
}
