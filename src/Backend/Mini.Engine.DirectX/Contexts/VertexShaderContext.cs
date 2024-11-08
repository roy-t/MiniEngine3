﻿using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.DirectX.Contexts;

public sealed class VertexShaderContext : DeviceContextPart
{
    public VertexShaderContext(DeviceContext context)
        : base(context) { }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.VSSetConstantBuffer(slot, buffer.Buffer);
    }

    public void SetShader(ILifetime<IVertexShader> shader)
    {
        var resource = this.DeviceContext.Resources.Get(shader).ID3D11Shader;
        this.ID3D11DeviceContext.VSSetShader(resource);
    }
    
    public void SetBuffer<T>(int slot, ShaderResourceView<T> view)
    where T : unmanaged
    {
        this.ID3D11DeviceContext.VSSetShaderResource(slot, view.View);
    }


    public void SetBuffer<T>(int slot, ILifetime<ShaderResourceView<T>> view)
        where T : unmanaged
    {
        this.SetBuffer(slot, this.DeviceContext.Resources.Get(view));
    }
}
