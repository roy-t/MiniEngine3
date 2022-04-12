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
        this.DepthBuffers = new DepthStencilBufferArray(device, DepthStencilFormat.D32_Float, resolution, resolution, this.Cascades.Length, entity.ToString(), nameof(CascadedShadowMapComponent));
    }

    public int Resolution { get; }
    public DepthStencilBufferArray DepthBuffers { get; private set; }

    public float[] Cascades { get; }

    public Matrix4x4 GlobalShadowMatrix { get; set; }

    public float[] Splits { get; }

    public Vector4[] Offsets { get; }

    public Vector4[] Scales { get; }

    public void Dispose()
    {
        this.DepthBuffers.Dispose();
    }
}
