using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using TileShader = Mini.Engine.Content.Shaders.Generated.Tiles;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed class TileRenderService : IDisposable
{
    private readonly TileShader Shader;
    private readonly TileShader.User User;

    private readonly RasterizerState CullCounterClockwise;
    private readonly RasterizerState CullNoneNoDepthClip;
    private readonly DepthStencilState ReverseZ;
    private readonly DepthStencilState ReverseZReadOnly;
    private readonly DepthStencilState Default;
    private readonly BlendState Opaque;
    private readonly SamplerState AnisotropicWrap;

    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<TileComponent> Tiles;

    public TileRenderService(Device device, TileShader shader, IComponentContainer<TransformComponent> transforms, IComponentContainer<TileComponent> tiles)
    {
        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.CullNoneNoDepthClip = device.RasterizerStates.CullNoneNoDepthClip;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.ReverseZReadOnly = device.DepthStencilStates.ReverseZReadOnly;
        this.Default = device.DepthStencilStates.Default;
        this.AnisotropicWrap = device.SamplerStates.AnisotropicWrap;
        this.Opaque = device.BlendStates.Opaque;

        this.Shader = shader;
        this.User = shader.CreateUserFor<TileRenderService>();

        this.Transforms = transforms;
        this.Tiles = tiles;
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

    /// <summary>
    /// Renders a single tile component, assumes device has been properly setup
    /// </summary>
    public void RenderTile(DeviceContext context, in TileComponent tile, in TransformComponent transform, in CameraComponent camera, in TransformComponent cameraTransform)
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
    /// Configures everything for rendering tile outlines, except for the output (render target)
    /// </summary>    
    public void SetupTileOutlineRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(null, PrimitiveTopology.LineStrip, this.Shader.VsLine, this.CullCounterClockwise, x, y, width, height, this.Shader.PsLine, this.Opaque, this.ReverseZReadOnly);
        context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
    }

    /// <summary>
    /// Renders a single outline for a tile component, assumes device has been properly setup
    /// </summary>
    public void RenderTileOutline(DeviceContext context, in TileComponent tile, in TransformComponent transform, in CameraComponent camera, in TransformComponent cameraTransform)
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
    /// Configures everything for rendering tile depths, except for the output (render target)
    /// </summary>   
    public void SetupTileDepthRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(null, PrimitiveTopology.TriangleStrip, this.Shader.VsDepth, this.CullNoneNoDepthClip, x, y, width, height, this.Shader.PsDepth, this.Opaque, this.Default);
        context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
    }

    /// <summary>
    /// Renders the depth of a single tile component, assumes device has been properly setup
    /// </summary>
    public void RenderTileDepth(DeviceContext context, in TileComponent tile, in TransformComponent transform, in Matrix4x4 viewProjection)
    {
        var world = transform.Current.GetMatrix();
        var worldViewProjection = world * viewProjection;

        this.User.MapConstants(context, Matrix4x4.Identity, worldViewProjection, Matrix4x4.Identity, Vector3.Zero, Vector2.Zero, Vector2.Zero, tile.Columns, tile.Rows);

        context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);
        context.DrawInstanced(4, (int)(tile.Columns * tile.Rows));
    }

    /// <summary>
    /// Calls SetupTileDepthRender and then draws all tile components
    /// </summary>
    public void SetupAndRenderAllTileDepths(DeviceContext context, int x, int y, int width, int height, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        this.SetupTileDepthRender(context, x, y, width, height);

        var iterator = this.Tiles.IterateAll();
        while (iterator.MoveNext())
        {
            ref var tile = ref iterator.Current;
            ref var transform = ref this.Transforms[tile.Entity];
            this.RenderTileDepth(context, in tile, in transform, in viewProjection);
        }
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}
