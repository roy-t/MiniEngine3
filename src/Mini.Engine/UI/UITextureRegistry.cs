using System;
using System.Collections.Generic;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.UI;

[Service]
public sealed class UITextureRegistry
{
    private readonly Dictionary<int, IntPtr> TexturesToPointers;
    private readonly Dictionary<IntPtr, WeakReference<ITexture2D>> PointerToTexture;
    private int textureCounter;

    public UITextureRegistry()
    {
        this.TexturesToPointers = new Dictionary<int, IntPtr>();
        this.PointerToTexture = new Dictionary<IntPtr, WeakReference<ITexture2D>>();
    }

    public IntPtr Get(ITexture2D texture)
    {
        if (this.TexturesToPointers.TryGetValue(texture.GetHashCode(), out var pointer))
        {
            return pointer;
        }

        return this.Register(texture);
    }

    public ITexture2D Get(IntPtr id)
    {
        if (this.PointerToTexture[id].TryGetTarget(out var texture))
        {
            return texture;
        }

        throw new Exception("Referenced texture no longer exists");
    }

    private IntPtr Register(ITexture2D texture)
    {
        var pointer = (IntPtr)this.textureCounter++;
        this.TexturesToPointers.Add(texture.GetHashCode(), pointer);
        this.PointerToTexture.Add(pointer, new WeakReference<ITexture2D>(texture));
        return pointer;
    }
}
