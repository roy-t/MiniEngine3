using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Geometry;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Models.Generators;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;


namespace Mini.Engine.Graphics.PBR;

[Service]
public partial class PointLightSystem : ISystem
{

    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly GeometryVs VertexShader;
    private readonly GeometryPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<CBuffer0> ConstantBuffer;
    private readonly IModel Sphere;

    public PointLightSystem(Device device, FrameService frameService, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<PointLightSystem>();
        this.FrameService = frameService;
        this.VertexShader = content.LoadGeometryVs();
        this.PixelShader = content.LoadGeometryPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<CBuffer0>(device, "constants_pointlightsystem");

        this.Sphere = SphereGenerator.Generate(device, 3, content.LoadDefaultMaterial(), "PointLight");
    }

    public void OnSet()
    {
        var width = this.FrameService.GBuffer.Width;
        var height = this.FrameService.GBuffer.Height;

        this.Context.IA.SetInputLayout(this.InputLayout);
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.Context.VS.SetShader(this.VertexShader);

        this.Context.RS.SetViewPort(0, 0, width, height);
        this.Context.RS.SetScissorRect(0, 0, width, height);
        //this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        this.Context.PS.SetShader(this.PixelShader);
        // TODO: use shader constants slots
        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(0, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(1, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(2, this.FrameService.GBuffer.Depth);
        this.Context.PS.SetShaderResource(3, this.FrameService.GBuffer.Material);

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Depth, this.FrameService.GBuffer.Normal);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawPointLight(PointLightComponent component, TransformComponent transform)
    {
        var camera = this.FrameService.Camera;                
        var isInside = Vector3.Distance(camera.Transform.Position, transform.Transform.Position) < component.RadiusOfInfluence;
        if (isInside)
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullClockwise);
        }
        else
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);
        }

        var world = Matrix4x4.CreateScale(component.RadiusOfInfluence) * transform.AsMatrix();
        var cBuffer = new CBuffer0()
        {
            WorldViewProjection = world * camera.ViewProjection,            
            CameraPosition = camera.Transform.Position,                                  
            // InverseViewProjection = Matrix4x4.Invert(camera.ViewProjection)
            // Position = transform.Transform.Position,
            // Color = component.Color,
            // Strength = component.Strength,
        };
        this.ConstantBuffer.MapData(this.Context, cBuffer);
        this.Context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);

        this.Context.IA.SetVertexBuffer(this.Sphere.Vertices);
        this.Context.IA.SetIndexBuffer(this.Sphere.Indices);

        this.Context.DrawIndexed(this.Sphere.Primitives[0].IndexCount, this.Sphere.Primitives[0].IndexOffset, 0);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }
}
