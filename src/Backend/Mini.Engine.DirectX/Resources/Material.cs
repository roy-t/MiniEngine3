namespace Mini.Engine.DirectX.Resources;

public sealed class Material : IMaterial
{
    public Material(ITexture2D albedo, ITexture2D metalicness, ITexture2D normal, ITexture2D roughness, ITexture2D ambientOcclusion, string name)
    {
        this.Name = name;
        this.Albedo = albedo;
        this.Metalicness = metalicness;
        this.Normal = normal;
        this.Roughness = roughness;
        this.AmbientOcclusion = ambientOcclusion;
    }

    public string Name { get; }
    public ITexture2D Albedo { get; }
    public ITexture2D Metalicness { get; }
    public ITexture2D Normal { get; }
    public ITexture2D Roughness { get; }
    public ITexture2D AmbientOcclusion { get; }

    public void Dispose()
    {
        this.Albedo.Dispose();
        this.Metalicness.Dispose();
        this.Normal.Dispose();
        this.Roughness.Dispose();
        this.AmbientOcclusion.Dispose();
    }
}
