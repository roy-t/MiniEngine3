using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

using Shader = Mini.Engine.Content.Shaders.Generated.Primitive;

namespace Mini.Engine.Graphics.Diesel;
[Service]
public sealed class PrimitiveSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly CameraService CameraService;

    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly InputLayout InputLayout;

    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<TransformComponent> Transforms;

    private readonly RasterizerState CullCounterClockwise;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;

    public PrimitiveSystem(Device device, CameraService cameraService, Shader shader, IComponentContainer<PrimitiveComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<PrimitiveSystem>();
        this.CameraService = cameraService;
        this.Primitives = models;
        this.Transforms = transforms;

        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.Opaque = device.BlendStates.Opaque;

        this.Shader = shader;
        this.User = shader.CreateUserFor<PrimitiveSystem>();
        this.InputLayout = shader.CreateInputLayoutForVs(PrimitiveVertex.Elements);
    }
    
    public Task<CommandList> DrawPrimitives(RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int heigth, float alpha)
    {
        return Task.Run(() =>
        {
            this.Context.OM.SetRenderTarget(albedo, depth);

            this.Context.Setup(this.InputLayout, Vortice.Direct3D.PrimitiveTopology.TriangleList, this.Shader.Vs, this.CullCounterClockwise, x, y, width, heigth, this.Shader.Ps, this.Opaque, this.ReverseZ);
            this.Context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
            this.Context.PS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);

            ref var camera = ref this.CameraService.GetPrimaryCamera();
            ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();

            var iterator = this.Primitives.IterateAll();
            while (iterator.MoveNext())
            {
                ref var model = ref iterator.Current;
                if (this.Transforms.Contains(model.Entity))
                {
                    ref var transform = ref this.Transforms[model.Entity].Value;
                    this.DrawPrimitive(in camera.Camera, in cameraTransform.Current, in model.Value, in transform.Current);
                }
            }

            return this.Context.FinishCommandList();
        });
    }


    private void DrawPrimitive(in PerspectiveCamera camera, in Transform cameraTransform, in PrimitiveComponent primitive, in Transform transform)
    {
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform);
        var viewVolume = new Frustum(viewProjection);

        var world = transform.GetMatrix();
        var mesh = this.Context.Resources.Get(primitive.Mesh);
        var bounds = mesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            this.Context.IA.SetVertexBuffer(mesh.Vertices);
            this.Context.IA.SetIndexBuffer(mesh.Indices);

            this.User.MapConstants(this.Context, world * viewProjection, world, primitive.Color);

            this.Context.DrawIndexed(mesh.Indices.Length, 0, 0);
        }
    }


    public void Dispose()
    {
        this.Context.Dispose();
        this.User.Dispose();
        this.InputLayout.Dispose();
    }
}
