using System.Drawing;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;

using Shader = Mini.Engine.Content.Shaders.Generated.Primitive;

namespace Mini.Engine.Graphics.Primitives;

[Service]
public sealed class PrimitiveRenderService : IDisposable
{
    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<InstancesComponent> Instances;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<ShadowCasterComponent> ShadowCasters;

    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly InputLayout InputLayout;
    private readonly InputLayout DepthInputLayout;

    private readonly RasterizerState CullCounterClockwise;
    private readonly RasterizerState CullNoneNoDepthClip;
    private readonly DepthStencilState Default;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;


    public PrimitiveRenderService(Device device, Shader shader, IComponentContainer<PrimitiveComponent> primitives, IComponentContainer<InstancesComponent> instances, IComponentContainer<TransformComponent> transforms, IComponentContainer<ShadowCasterComponent> shadowCasters)
    {
        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.CullNoneNoDepthClip = device.RasterizerStates.CullNoneNoDepthClip;
        this.Default = device.DepthStencilStates.Default;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.Opaque = device.BlendStates.Opaque;

        this.Shader = shader;
        this.User = shader.CreateUserFor<PrimitiveSystem>();
        this.InputLayout = shader.CreateInputLayoutForVsinstanced(PrimitiveVertex.Elements);
        this.DepthInputLayout = shader.CreateInputLayoutForVsDepthInstanced(PrimitiveVertex.Elements);
        this.Primitives = primitives;
        this.Instances = instances;
        this.Transforms = transforms;
        this.ShadowCasters = shadowCasters;
    }

    public void Setup(DeviceContext context, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.Shader.Vsinstanced, this.CullCounterClockwise, in viewport, in scissor, this.Shader.Ps, this.Opaque, this.ReverseZ);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
    }

    public void Render(DeviceContext context, in CameraComponent cameraComponent, in TransformComponent cameraTransformComponent, in PrimitiveComponent primitiveComponent, in InstancesComponent instancesComponent, in TransformComponent transformComponent)
    {
        var mesh = context.Resources.Get(primitiveComponent.Mesh);
        var world = transformComponent.Current.GetMatrix();

        var viewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Current, cameraComponent.Jitter);

        var previousViewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Previous, cameraComponent.PreviousJitter);
        var previousWorld = transformComponent.Previous.GetMatrix();

        var cameraPosition = cameraTransformComponent.Current.GetPosition();

        context.IA.SetVertexBuffer(mesh.Vertices);
        context.IA.SetIndexBuffer(mesh.Indices);
        context.VS.SetBuffer(Shader.Parts, mesh.PartsView);
        context.VS.SetBuffer(Shader.Instances, instancesComponent.InstanceBufferView);

        this.User.MapConstants(context, previousViewProjection, viewProjection, previousWorld, world, cameraPosition, cameraComponent.PreviousJitter, cameraComponent.Jitter, (uint)mesh.PartCount);

        context.DrawIndexedInstanced(mesh.IndexCount, instancesComponent.InstanceCount);        
    }

    public void SetupAndRenderAllPrimitiveDepths(DeviceContext context, float importanceThreshold, in Rectangle viewport, in Rectangle scissor, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        this.SetupDepth(context, in viewport, in scissor);

        foreach (ref var component in this.Primitives.IterateAll())
        {
            var entity = component.Entity;
            if (entity.HasComponents(this.ShadowCasters, this.Instances, this.Transforms))
            {
                ref var primitive = ref component.Value;
                ref var instances = ref this.Instances[entity].Value;
                ref var transform = ref this.Transforms[entity].Value;
                ref var shadowCaster = ref this.ShadowCasters[entity].Value;
                if (shadowCaster.Importance >= importanceThreshold)
                {
                    this.RenderDepth(context, in primitive, in instances, in transform, in viewVolume, in viewProjection);
                }
            }
        }
    }

    private void RenderDepth(DeviceContext context, in PrimitiveComponent primitiveComponent, in InstancesComponent instancesComponent, in TransformComponent transformComponent, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        var mesh = context.Resources.Get(primitiveComponent.Mesh);
        var world = transformComponent.Current.GetMatrix();        

        context.IA.SetVertexBuffer(mesh.Vertices);
        context.IA.SetIndexBuffer(mesh.Indices);
        context.VS.SetBuffer(Shader.Instances, instancesComponent.InstanceBufferView);

        this.User.MapDepthConstants(context, viewProjection, world);

        context.DrawIndexedInstanced(mesh.IndexCount, instancesComponent.InstanceCount);
    }

    private void SetupDepth(DeviceContext context, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.DepthInputLayout, PrimitiveTopology.TriangleList, this.Shader.VsDepthInstanced, this.CullNoneNoDepthClip, in viewport, in scissor, this.Shader.PsDepth, this.Opaque, this.Default);
        context.VS.SetConstantBuffer(Shader.DepthConstantsSlot, this.User.DepthConstantsBuffer);
        context.PS.ClearShader();
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
        this.DepthInputLayout.Dispose();
    }
}
