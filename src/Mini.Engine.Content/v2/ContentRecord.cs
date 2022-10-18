﻿using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.v2;
internal sealed class ContentRecord
{    
    public ContentRecord(TextureLoaderSettings settings)
        : this()
    {
        this.TextureSettings = settings;
    }

    public ContentRecord(MaterialLoaderSettings settings)
    : this()
    {
        this.MaterialSettings = settings;
    }

    public ContentRecord(ModelLoaderSettings settings)
    : this()
    {
        this.ModelSettings = settings;
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
