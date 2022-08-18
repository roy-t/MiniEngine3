using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
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

    public void SetShader(IPixelShader shader)
    {
        this.ID3D11DeviceContext.PSSetShader(shader.ID3D11Shader);
    }

    //[Obsolete("Use IResource<T> overloads")]
    public void SetShaderResource(int slot, ISurface texture)
    {
        this.ID3D11DeviceContext.PSSetShaderResource(slot, texture.ShaderResourceView);
    }    

    public void SetShaderResource(int slot, IResource<ISurface> texture)
    {
        var srv = this.DeviceContext.Resources.Get(texture).ShaderResourceView;
        this.ID3D11DeviceContext.PSSetShaderResource(slot, srv);
    }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.PSSetConstantBuffer(slot, buffer.Buffer);
    }
}
