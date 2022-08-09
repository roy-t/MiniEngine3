using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public struct CascadedShadowMapComponent : IComponent
{
    public int Resolution;        
    public Vector4 Cascades;
    public Vector4 Splits;
    public Matrix4x4 Scales;
    public Matrix4x4 Offsets;
    public Matrix4x4 GlobalShadowMatrix;

    public DepthStencilBufferArray DepthBuffers;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(Device device, int resolution, float cascade0, float cascade1, float cascade2, float cascade3)        
    {
        this.Resolution = resolution;

        this.Cascades.X = cascade0;
        this.Cascades.Y = cascade1;
        this.Cascades.Z = cascade2;
        this.Cascades.W = cascade3;        
        
        this.DepthBuffers = new DepthStencilBufferArray(device, DepthStencilFormat.D32_Float, resolution, resolution, 4, this.Entity.ToString(), nameof(CascadedShadowMapComponent));
    }

    public void Destroy()
    {
        this.DepthBuffers.Dispose();
    }
}
