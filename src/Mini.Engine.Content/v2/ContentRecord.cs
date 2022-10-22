using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.v2;
public sealed class ContentRecord
{
    public static ContentRecord Default => new ContentRecord();

    public ContentRecord(TextureLoaderSettings? settings)
        : this()
    {
        this.TextureSettings = settings ?? TextureLoaderSettings.Default;
    }

    public ContentRecord(MaterialLoaderSettings? settings)
    : this()
    {
        this.MaterialSettings = settings ?? MaterialLoaderSettings.Default;
    }

    public ContentRecord(ModelLoaderSettings? settings)
    : this()
    {
        this.ModelSettings = settings ?? ModelLoaderSettings.Default;
    }

    public ContentRecord(TextureLoaderSettings texture, MaterialLoaderSettings material, ModelLoaderSettings model)
    {
        this.TextureSettings = texture;
        this.MaterialSettings = material;
        this.ModelSettings = model;
    }

    private ContentRecord()
    {
        this.TextureSettings = TextureLoaderSettings.Default;
        this.MaterialSettings = MaterialLoaderSettings.Default;
        this.ModelSettings = ModelLoaderSettings.Default;
    }    

    public TextureLoaderSettings TextureSettings { get; }
    public MaterialLoaderSettings MaterialSettings { get; }
    public ModelLoaderSettings ModelSettings { get; }
}
