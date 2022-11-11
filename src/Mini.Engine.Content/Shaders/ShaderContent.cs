using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Content;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.Content.Shaders;
internal abstract class ShaderContent<TShader, TSettings> : IContent<TShader, TSettings>
    where TShader : IShader, IDisposable
{
    protected TShader original;

    public ShaderContent(ContentId id, TShader original, TSettings settings, ISet<string> dependencies)
    {
        this.Id = id;
        this.Settings = settings;
        this.Dependencies = dependencies;

        this.Reload(original);
    }

    public ContentId Id { get; }
    public ISet<string> Dependencies { get; }

    public TSettings Settings { get; }

    public string Name => this.original.Name;

    [MemberNotNull(nameof(original))]
    public void Reload(TShader original)
    {
        this.Dispose();
        this.original = original;
    }

    public void Dispose()
    {
        this.original?.Dispose();
    }
}
