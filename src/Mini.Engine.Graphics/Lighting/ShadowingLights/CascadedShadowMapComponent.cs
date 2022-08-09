using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public struct CascadedShadowMapComponent : IComponent
{
    public void Init(Device device, int resolution, float cascade0, float cascade1, float cascade2, float cascade3)        
    {
        this.Resolution = resolution;

        this.Cascades = new float[] { cascade0, cascade1, cascade2, cascade3 };
        this.GlobalShadowMatrix = Matrix4x4.Identity;
        this.Splits = new float[this.Cascades.Length];
        this.Offsets = new Vector4[this.Cascades.Length];
        this.Scales = new Vector4[this.Cascades.Length];
        this.DepthBuffers = new DepthStencilBufferArray(device, DepthStencilFormat.D32_Float, resolution, resolution, this.Cascades.Length, this.Entity.ToString(), nameof(CascadedShadowMapComponent));
    }

    public int Resolution;
    public DepthStencilBufferArray DepthBuffers;
    public float[] Cascades;
    public Matrix4x4 GlobalShadowMatrix;
    public float[] Splits;
    public Vector4[] Offsets;
    public Vector4[] Scales;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy()
    {
        this.DepthBuffers.Dispose();
    }
}
