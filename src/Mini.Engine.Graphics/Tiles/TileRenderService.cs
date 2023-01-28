using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Transforms;

using TileShader = Mini.Engine.Content.Shaders.Generated.Tiles;
using Vortice.Direct3D;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Cameras;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed class TileRenderService : IDisposable
{
    //private readonly FrameService FrameService;

    private readonly TileShader Shader;
    private readonly TileShader.User User;

    private readonly RasterizerState CullCounterClockwise;
    private readonly BlendState Opaque;
    private readonly DepthStencilState ReverseZ;
    private readonly DepthStencilState ReverseZReadOnly;
    private readonly SamplerState AnisotropicWrap;

    public TileRenderService(Device device, TileShader shader)
    {
        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.Opaque = device.BlendStates.Opaque;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.ReverseZReadOnly = device.DepthStencilStates.ReverseZReadOnly;
        this.AnisotropicWrap = device.SamplerStates.AnisotropicWrap;

        this.Shader = shader;
        this.User = shader.CreateUserFor<TileRenderService>();
    }

    /// <summary>
    /// Configures everything for rendering tiles, except for the output (render target)
    /// </summary>    
    public void SetupTileRender(DeviceContext context, int x, int y, int width, int height)
    {        
        context.Setup(null, PrimitiveTopology.TriangleStrip, this.Shader.Vs, this.CullCounterClockwise, x, y, width, height, this.Shader.Ps, this.Opaque, this.ReverseZ);                
        context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetSampler(TileShader.TextureSampler, this.AnisotropicWrap);       
    }

    public void RenderTile(DeviceContext context, ref TileComponent tile, ref TransformComponent transform, ref CameraComponent camera, ref TransformComponent cameraTransform)
    {
        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, camera.PreviousJitter);
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, camera.Jitter);
        var cameraPosition = cameraTransform.Current.GetPosition();

        this.User.MapConstants(context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, camera.PreviousJitter, camera.Jitter, tile.Columns, tile.Rows);

        var material = context.Resources.Get(tile.Material);
        context.PS.SetShaderResource(TileShader.Albedo, material.Albedo);
        context.PS.SetShaderResource(TileShader.Normal, material.Normal);
        context.PS.SetShaderResource(TileShader.Metalicness, material.Metalicness);
        context.PS.SetShaderResource(TileShader.Roughness, material.Roughness);
        context.PS.SetShaderResource(TileShader.AmbientOcclusion, material.AmbientOcclusion);

        context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);
        context.DrawInstanced(4, (int)(tile.Columns * tile.Rows));
    }

    /// <summary>
    /// Configures everything for rendering tiles, except for the output (render target)
    /// </summary>    
    public void SetupTileOutlineRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(null, PrimitiveTopology.LineStrip, this.Shader.VsLine, this.CullCounterClockwise, x, y, width, height, this.Shader.PsLine, this.Opaque, this.ReverseZReadOnly);
        context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
    }

    public void RenderTileOutline(DeviceContext context, ref TileComponent tile, ref TransformComponent transform, ref CameraComponent camera, ref TransformComponent cameraTransform)
    {
        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, camera.PreviousJitter);
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, camera.Jitter);
        var cameraPosition = cameraTransform.Current.GetPosition();

        this.User.MapConstants(context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, camera.PreviousJitter, camera.Jitter, tile.Columns, tile.Rows);

        context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);
        context.DrawInstanced(5, (int)(tile.Columns * tile.Rows));
    }

    /// <summary>
    /// Configures everything for rendering the depth of the tiles tiles (for shadowing), except for the output (render target)
    /// </summary>   
    public void SetupTileDepthRender(DeviceContext context, int x, int y, int width, int height)
    {                
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}
