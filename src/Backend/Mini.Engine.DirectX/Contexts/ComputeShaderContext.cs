using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
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

    public void SetShader(IComputeShader shader)
    {
        this.ID3D11DeviceContext.CSSetShader(shader.ID3D11Shader);
    }

    public void SetShaderResource(int slot, ITexture texture)
    {
        this.ID3D11DeviceContext.CSSetShaderResource(slot, texture.ShaderResourceView);
    }

    public void SetShaderResource<T>(int slot, StructuredBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetShaderResource(slot, buffer.GetShaderResourceView());
    }

    public void SetShaderResource<T>(int slot, StructuredBuffer<T> buffer, int firstElement, int length)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetShaderResource(slot, buffer.GetShaderResourceView(firstElement, length));
    }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetConstantBuffer(slot, buffer.Buffer);
    }   

    public void SetUnorderedAccessView(int slot, RWTexture2D texture)
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, texture.UnorderedAccessViews[0]);
    }

    public void SetUnorderedAccessView(int slot, RWTexture2D texture, int mipMapSlice)
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, texture.UnorderedAccessViews[mipMapSlice]);
    }

    public void SetUnorderedAccessView<T>(int slot, RWStructuredBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, buffer.GetUnorderedAccessView());
    }    

    public void SetUnorderedAccessView<T>(int slot, RWStructuredBuffer<T> buffer, int firstElement, int length)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.CSSetUnorderedAccessView(slot, buffer.GetUnorderedAccessView(firstElement, length));
    }   

    public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
    {
        this.ID3D11DeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
    }
}
