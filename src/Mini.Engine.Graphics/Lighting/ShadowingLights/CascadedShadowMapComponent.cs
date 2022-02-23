using System;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public sealed class CascadedShadowMapComponent : Component, IDisposable
{
    public CascadedShadowMapComponent(Entity entity, Device device, int resolution, float cascade0, float cascade1, float cascade2, float cascade3)
        : base(entity)
    {
        this.Resolution = resolution;

        this.Cascades = new float[] { cascade0, cascade1, cascade2, cascade3 };
        this.GlobalShadowMatrix = Matrix4x4.Identity;
        this.Splits = new float[this.Cascades.Length];
        this.Offsets = new Vector4[this.Cascades.Length];
        this.Scales = new Vector4[this.Cascades.Length];


        this.RenderTargets = new RenderTarget2D[this.Cascades.Length];
        this.DepthBuffers = new DepthStencilBuffer[this.Cascades.Length];

        for(var i = 0; i < this.Cascades.Length; i++)
        {
            this.RenderTargets[i] = new RenderTarget2D(device, resolution, resolution, Vortice.DXGI.Format.R32_Float, $"{entity}_{i}_CascadedShadowMap_RT");
            this.DepthBuffers[i] = new DepthStencilBuffer(device, DepthStencilFormat.D32_Float, resolution, resolution, $"{entity}_{i}_Depth");
        }        
    }

    public int Resolution { get; }

    // TODO: do we really need these render targets, even if we're only interested in the depth buffer output?
    // the other way around, maybe we can only use 1 depth buffer in the system if we clear it in between?
    public RenderTarget2D[] RenderTargets { get; private set; }
    public DepthStencilBuffer[] DepthBuffers { get; private set; }

    public float[] Cascades { get; }

    public Matrix4x4 GlobalShadowMatrix { get; set; }

    public float[] Splits { get; }

    public Vector4[] Offsets { get; }

    public Vector4[] Scales { get; }

    public void Dispose()
    {
        for (var i = 0; i < this.Cascades.Length; i++)
        {
            this.RenderTargets[i].Dispose();
            this.DepthBuffers[i].Dispose();
        }
    }
}
