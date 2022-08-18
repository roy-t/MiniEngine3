using Mini.Engine.DirectX.Resources.vNext;

namespace Mini.Engine.DirectX.Resources;

public sealed class Material : IMaterial
{
    public Material(ITexture albedo, ITexture metalicness, ITexture normal, ITexture roughness, ITexture ambientOcclusion, string user)
    {
        this.Name = DebugNameGenerator.GetName(user);
        this.Albedo = albedo;
        this.Metalicness = metalicness;
        this.Normal = normal;
        this.Roughness = roughness;
        this.AmbientOcclusion = ambientOcclusion;
    }

    public string Name { get; }
    public ITexture Albedo { get; }
    public ITexture Metalicness { get; }
    public ITexture Normal { get; }
    public ITexture Roughness { get; }
    public ITexture AmbientOcclusion { get; }

    public void Dispose()
    {
        this.Albedo.Dispose();
        this.Metalicness.Dispose();
        this.Normal.Dispose();
        this.Roughness.Dispose();
        this.AmbientOcclusion.Dispose();
    }
}
