using System.Drawing;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Transforms;
using Shader = Mini.Engine.Content.Shaders.Generated.Primitive;

namespace Mini.Engine.Graphics.Diesel;

[Service]
public sealed class PrimitiveRenderService : IDisposable
{
    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly InputLayout InputLayout;

    private readonly RasterizerState CullCounterClockwise;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;


    public PrimitiveRenderService(Device device, Shader shader)
    {
        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.Opaque = device.BlendStates.Opaque;

        this.Shader = shader;
        this.User = shader.CreateUserFor<PrimitiveSystem>();
        this.InputLayout = shader.CreateInputLayoutForVsinstanced(PrimitiveVertex.Elements);
    }      

    public void Setup(DeviceContext context, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.InputLayout, Vortice.Direct3D.PrimitiveTopology.TriangleList, this.Shader.Vsinstanced, this.CullCounterClockwise, in viewport, in scissor, this.Shader.Ps, this.Opaque, this.ReverseZ);

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
        context.VS.SetBuffer(Shader.Parts, mesh.Parts);
        context.VS.SetBuffer(Shader.Instances, instancesComponent.InstanceBuffer);
                
        this.User.MapConstants(context, previousViewProjection, viewProjection, previousWorld, world, cameraPosition, cameraComponent.PreviousJitter, cameraComponent.Jitter, (uint)mesh.PartCount);

        context.DrawIndexedInstanced(mesh.IndexCount, instancesComponent.InstanceCount);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
    }
}
