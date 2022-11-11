using Mini.Engine.Content.Materials;

namespace Mini.Engine.Content.Models;

public sealed record ModelSettings(MaterialSettings MaterialSettings)
{
    public static readonly ModelSettings Default = new(MaterialSettings.Default);
}
