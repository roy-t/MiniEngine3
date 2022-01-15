using System;
using System.Collections.Generic;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.UI;

[Service]
public sealed class UITextureRegistry
{    
    private readonly Dictionary<IntPtr, ITexture2D> TextureResources;
    private int textureCounter;

    public UITextureRegistry()
    {
        this.TextureResources = new Dictionary<IntPtr, ITexture2D>();
    }

    public IntPtr Register(ITexture2D texture)
    {
        var id = (IntPtr)this.textureCounter++;
        this.TextureResources.Add(id, texture);

        return id;
    }

    public ITexture2D Get(IntPtr id)
    {
        return this.TextureResources[id];
    }
}
