using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class ComputeShaderContext : DeviceContextPart
{
    public ComputeShaderContext(DeviceContext context)
        : base(context) { }

    public void SetSampler(int slot, SamplerState sampler)
    {
        this.ID3D11DeviceContext.CSSetSampler(slot, sampler.State);
    }

    public void SetSamplers(int startSlot, params SamplerState[] samplers)
    {
        var nativeSamplers = new ID3D11SamplerState[samplers.Length];
        for (var i = 0; i < samplers.Length; i++)
        {
            nativeSamplers[i] = samplers[i].State;
        }

        this.ID3D11DeviceContext.CSSetSamplers(startSlot, nativeSamplers);
    }

    public void SetShader(ILifetime<IComputeShader> shader)
    {
        var resource = this.DeviceContext.Resources.Get(shader).ID3D11Shader;
        this.ID3D11DeviceContext.CSSetShader(resource);
    }

    public void SetShaderResource(int slot, ISurface texture)
    {
        this.ID3D11DeviceContext.CSSetShaderResource(slot, texture.ShaderResourceView);
    }

    public void SetShaderResource<T>(int slot, ShaderResourceView<T> view)
        where T : unmanaged
    {        
        this.ID3D11DeviceContext.CSSetShaderResource(slot, view.View);
    }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetConstantBuffer(slot, buffer.Buffer);
    }

    public void ClearUnorderedAccessView(int slot)
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, null);
    }

    public void SetUnorderedAccessView(int slot, IRWTexture texture)
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, texture.UnorderedAccessViews[0]);
    }

    public void SetUnorderedAccessView(int slot, IRWTexture texture, int mipMapSlice)
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, texture.UnorderedAccessViews[mipMapSlice]);
    }

    public void SetUnorderedAccessView<T>(int slot, UnorderedAccessView<T> view)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, view.View);
    }    

    public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
    {
        this.ID3D11DeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
    }
}
