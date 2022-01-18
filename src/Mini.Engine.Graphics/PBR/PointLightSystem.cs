using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.PointLight;
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
    private readonly PointLightVs VertexShader;
    private readonly PointLightPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;
    private readonly ConstantBuffer<PerLightConstants> PerLightConstantBuffer;
    private readonly IModel Sphere;

    public PointLightSystem(Device device, FrameService frameService, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<PointLightSystem>();
        this.FrameService = frameService;
        this.VertexShader = content.LoadPointLightVs();
        this.PixelShader = content.LoadPointLightPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, "PointLightSystem_Constants");
        this.PerLightConstantBuffer = new ConstantBuffer<PerLightConstants>(device, "PointLightSystem_PerLightConstants");

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

        this.Context.PS.SetShader(this.PixelShader);
        
        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(PointLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(PointLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(PointLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(PointLight.Material, this.FrameService.GBuffer.Material);

        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Additive);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        var camera = this.FrameService.Camera;
        Matrix4x4.Invert(camera.ViewProjection, out var inverseViewProjection);
        var cBuffer = new Constants()
        {
            InverseViewProjection = inverseViewProjection,
            CameraPosition = camera.Transform.Position
        };
        this.ConstantBuffer.MapData(this.Context, cBuffer);
        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
        this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawPointLight(PointLightComponent component, TransformComponent transform)
    {
        var camera = this.FrameService.Camera;
        var isInside = Vector3.Distance(camera.Transform.Position, transform.Transform.Position) < component.RadiusOfInfluence;
        if (isInside)
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwiseNoDepthClip);
        }
        else
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwiseNoDepthClip);
        }        

        var world = Matrix4x4.CreateScale(component.RadiusOfInfluence) * transform.AsMatrix();
        
        var cBuffer = new PerLightConstants()
        {            
            WorldViewProjection = world * camera.ViewProjection,                        
            LightPosition = transform.Transform.Position,
            Color = component.Color,
            Strength = component.Strength,
        };
        this.PerLightConstantBuffer.MapData(this.Context, cBuffer);
        this.Context.VS.SetConstantBuffer(PerLightConstants.Slot, this.PerLightConstantBuffer);
        this.Context.PS.SetConstantBuffer(PerLightConstants.Slot, this.PerLightConstantBuffer);

        this.Context.IA.SetVertexBuffer(this.Sphere.Vertices);
        this.Context.IA.SetIndexBuffer(this.Sphere.Indices);

        this.Context.DrawIndexed(this.Sphere.Primitives[0].IndexCount, this.Sphere.Primitives[0].IndexOffset, 0);
        // TODO: create a separate cbuffer for the PS so we need less updates
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }
}
