using System;
using System.Collections.Generic;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.UI;

[Service]
public sealed class UITextureRegistry
{
    private readonly Dictionary<int, IntPtr> TexturesToPointers;
    private readonly Dictionary<IntPtr, WeakReference<ISurface>> PointerToTexture;
    private int textureCounter;

    public UITextureRegistry()
    {
        this.TexturesToPointers = new Dictionary<int, IntPtr>();
        this.PointerToTexture = new Dictionary<IntPtr, WeakReference<ISurface>>();
    }

    public IntPtr Get(ISurface texture)
    {
        if (this.TexturesToPointers.TryGetValue(texture.GetHashCode(), out var pointer))
        {
            return pointer;
        }

        return this.Register(texture);
    }

    public ISurface Get(IntPtr id)
    {
        if (this.PointerToTexture[id].TryGetTarget(out var texture))
        {
            return texture;
        }

        throw new Exception("Referenced texture no longer exists");
    }

    private IntPtr Register(ISurface texture)
    {
        var pointer = (IntPtr)this.textureCounter++;
        this.TexturesToPointers.Add(texture.GetHashCode(), pointer);
        this.PointerToTexture.Add(pointer, new WeakReference<ISurface>(texture));
        return pointer;
    }
}
