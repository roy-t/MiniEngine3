using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class PixelShaderContext : DeviceContextPart
{
    public PixelShaderContext(DeviceContext context)
        : base(context) { }

    public void SetSampler(int slot, SamplerState sampler)
    {
        this.ID3D11DeviceContext.PSSetSampler(slot, sampler.State);
    }

    public void SetSamplers(int startSlot, params SamplerState[] samplers)
    {
        var nativeSamplers = new ID3D11SamplerState[samplers.Length];
        for (var i = 0; i < samplers.Length; i++)
        {
            nativeSamplers[i] = samplers[i].State;
        }

        this.ID3D11DeviceContext.PSSetSamplers(startSlot, nativeSamplers);
    }

    public void ClearShader()
    {
        this.ID3D11DeviceContext.PSSetShader(null, null, 0);
    }

    public void SetShader(ILifetime<IPixelShader> shader)
    {
        var resource = this.DeviceContext.Resources.Get(shader).ID3D11Shader;
        this.ID3D11DeviceContext.PSSetShader(resource);
    }

    //[Obsolete("Use IResource<T> overloads")]
    public void SetShaderResource(int slot, ISurface texture)
    {
        this.ID3D11DeviceContext.PSSetShaderResource(slot, texture.ShaderResourceView);
    }

    public void SetShaderResource(int slot, ILifetime<ISurface> texture)
    {
        var srv = this.DeviceContext.Resources.Get(texture).ShaderResourceView;
        this.ID3D11DeviceContext.PSSetShaderResource(slot, srv);
    }

    public void ClearShaderResource(int slot)
    {
        this.ID3D11DeviceContext.PSUnsetShaderResource(slot);
    }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.PSSetConstantBuffer(slot, buffer.Buffer);
    }


    public void SetBuffer<T>(int slot, ShaderResourceView<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.PSSetShaderResource(slot, buffer.View);
    }

    public void SetInstanceBuffer<T>(int slot, ILifetime<ShaderResourceView<T>> instanceBufferView)
       where T : unmanaged
    {
        var resource = this.DeviceContext.Resources.Get(instanceBufferView);
        this.ID3D11DeviceContext.PSSetShaderResource(slot, resource.View);
    }
}
