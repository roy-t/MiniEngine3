using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
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
        this.InputLayout = shader.CreateInputLayoutForVs(PrimitiveVertex.Elements);
    }


    public void Setup(DeviceContext context, RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int heigth)
    {
        context.OM.SetRenderTarget(albedo, depth);

        context.Setup(this.InputLayout, Vortice.Direct3D.PrimitiveTopology.TriangleList, this.Shader.Vs, this.CullCounterClockwise, x, y, width, heigth, this.Shader.Ps, this.Opaque, this.ReverseZ);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
    }    

    public void Render(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in PrimitiveComponent primitive, in Transform transform)
    {
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform);
        var viewVolume = new Frustum(viewProjection);

        var world = transform.GetMatrix();
        var mesh = context.Resources.Get(primitive.Mesh);
        var bounds = mesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            context.IA.SetVertexBuffer(mesh.Vertices);
            context.IA.SetIndexBuffer(mesh.Indices);

            this.User.MapConstants(context, world * viewProjection, world, cameraTransform.GetPosition(), primitive.Color);

            context.DrawIndexed(mesh.Indices.Length, 0, 0);
        }
    }


    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
    }
}
