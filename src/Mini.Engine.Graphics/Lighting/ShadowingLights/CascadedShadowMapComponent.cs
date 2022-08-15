using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public struct CascadedShadowMapComponent : IComponent
{
    public int Resolution;
    public IResource<IDepthStencilBufferArray> DepthBuffers;
    public Vector4 Cascades;
    public Vector4 Splits;
    public Matrix4x4 Scales;
    public Matrix4x4 Offsets;
    public Matrix4x4 GlobalShadowMatrix;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(IResource<IDepthStencilBufferArray> depthStencilBuffers, int resolution, float cascade0, float cascade1, float cascade2, float cascade3)        
    {
        this.Resolution = resolution;

        this.Cascades.X = cascade0;
        this.Cascades.Y = cascade1;
        this.Cascades.Z = cascade2;
        this.Cascades.W = cascade3;

        this.DepthBuffers = depthStencilBuffers;
    }
}
