using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using ShadowMap = Mini.Engine.Content.Shaders.Generated.ShadowMap;
using Terrain = Mini.Engine.Content.Shaders.Generated.Terrain;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainRenderService : IDisposable
{
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<TerrainComponent> TerrainContainer;

    private readonly Terrain Shader;
    private readonly Terrain.User User;
    private readonly InputLayout InputLayout;

    private readonly ShadowMap ShadowMapShader;
    private readonly ShadowMap.User ShadowMapUser;
    private readonly InputLayout ShadowMapInputLayout;

    private readonly RasterizerState CullCounterClockwise;
    private readonly RasterizerState CullNoneNoDepthClip;
    private readonly DepthStencilState Default;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;
    private readonly SamplerState AnisotropicWrap;

    public TerrainRenderService(Device device, Terrain shader, ShadowMap shadowMapShader, IComponentContainer<TransformComponent> transforms, IComponentContainer<TerrainComponent> terrain)
    {
        this.Transforms = transforms;
        this.TerrainContainer = terrain;

        this.Shader = shader;
        this.User = shader.CreateUserFor<TerrainRenderService>();
        this.InputLayout = shader.CreateInputLayoutForVs(ModelVertex.Elements);

        this.ShadowMapShader = shadowMapShader;
        this.ShadowMapUser = shadowMapShader.CreateUserFor<ModelRenderService>();
        this.ShadowMapInputLayout = this.ShadowMapShader.CreateInputLayoutForVs(ModelVertex.Elements);

        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.CullNoneNoDepthClip = device.RasterizerStates.CullNoneNoDepthClip;
        this.Default = device.DepthStencilStates.Default;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.AnisotropicWrap = device.SamplerStates.AnisotropicWrap;
        this.Opaque = device.BlendStates.Opaque;
    }

    /// <summary>
    /// Configures everything for rendering tiles, except for the output (render target)
    /// </summary>    
    public void SetupTerrainRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.CullCounterClockwise, x, y, width, height, this.Shader.Ps, this.Opaque, this.ReverseZ);

        context.VS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetSampler(Terrain.TextureSampler, this.AnisotropicWrap);
    }

    /// <summary>
    /// Renders a single terrain component, assumes device has been properly setup
    /// </summary>
    public void RenderTerrain(DeviceContext context, in TerrainComponent terrainComponent, in TransformComponent transformComponent, in CameraComponent cameraComponent, in TransformComponent cameraTransformComponent)
    {
        var viewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Current, cameraComponent.Jitter);
        var viewVolume = new Frustum(viewProjection);

        var mesh = context.Resources.Get(terrainComponent.Mesh);
        var world = transformComponent.Current.GetMatrix();
        var bounds = mesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            var normals = context.Resources.Get(terrainComponent.Normals);
            var erosion = context.Resources.Get(terrainComponent.Erosion);
            var material = context.Resources.Get(terrainComponent.Material);

            context.PS.SetShaderResource(Terrain.HeigthMapNormal, normals);
            context.PS.SetShaderResource(Terrain.Erosion, erosion);
            context.PS.SetShaderResource(Terrain.Albedo, material.Albedo);
            context.PS.SetShaderResource(Terrain.Normal, material.Normal);
            context.PS.SetShaderResource(Terrain.Metalicness, material.Metalicness);
            context.PS.SetShaderResource(Terrain.Roughness, material.Roughness);
            context.PS.SetShaderResource(Terrain.AmbientOcclusion, material.AmbientOcclusion);
            context.PS.SetShaderResource(Terrain.Foilage, terrainComponent.Foilage);

            var erosionColor = terrainComponent.ErosionColor;
            var depositionColor = terrainComponent.DepositionColor;
            var erosionColorMultiplier = terrainComponent.ErosionColorMultiplier;

            var previousViewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Previous, cameraComponent.PreviousJitter);
            var previousWorld = transformComponent.Previous.GetMatrix();
            var previousWorldViewProjection = previousWorld * previousViewProjection;
            var worldViewProjection = world * viewProjection;

            this.User.MapConstants(context, previousWorldViewProjection, worldViewProjection, world, cameraTransformComponent.Current.GetPosition(), cameraComponent.PreviousJitter, cameraComponent.Jitter, depositionColor, erosionColor, erosionColorMultiplier);

            context.IA.SetVertexBuffer(mesh.Vertices);
            context.IA.SetIndexBuffer(mesh.Indices);

            context.DrawIndexed(mesh.Indices.Length, 0, 0);
        }
    }

    /// <summary>
    /// Configures everything for rendering tile depths, except for the output (render target)
    /// </summary> 
    public void SetupTerrainDepthRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(this.ShadowMapInputLayout, PrimitiveTopology.TriangleList, this.ShadowMapShader.Vs, this.CullNoneNoDepthClip, x, y, width, height, this.ShadowMapShader.Ps, this.Opaque, this.Default);
        context.VS.SetConstantBuffer(ShadowMap.ConstantsSlot, this.ShadowMapUser.ConstantsBuffer);
        //context.PS.SetSampler(ShadowMap.TextureSampler, this.AnisotropicWrap);
        context.PS.ClearShader();
    }

    /// <summary>
    /// Renders the depth of a single terrain component, assumes device has been properly setup
    /// </summary>
    public void RenderTerrainDepth(DeviceContext context, in TerrainComponent terrainComponent, in TransformComponent transformComponent, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        var mesh = context.Resources.Get(terrainComponent.Mesh);
        var world = transformComponent.Current.GetMatrix();
        var bounds = mesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            this.ShadowMapUser.MapConstants(context, world * viewProjection);

            context.IA.SetVertexBuffer(mesh.Vertices);
            context.IA.SetIndexBuffer(mesh.Indices);

            var material = context.Resources.Get(terrainComponent.Material);
            context.PS.SetShaderResource(ShadowMap.Albedo, material.Albedo);
            context.DrawIndexed(mesh.Indices.Length, 0, 0);
        }
    }

    /// <summary>
    /// Calls SetupTerrainDepthRender and then draws all terrain components
    /// </summary>
    public void SetupAndRenderAllTerrainDepths(DeviceContext context, int x, int y, int width, int height, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        this.SetupTerrainDepthRender(context, x, y, width, height);

        var iterator = this.TerrainContainer.IterateAll();
        while (iterator.MoveNext())
        {
            ref var terrain = ref iterator.Current;
            ref var transform = ref this.Transforms[terrain.Entity];
            this.RenderTerrainDepth(context, in terrain, in transform, in viewVolume, in viewProjection);
        }
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.ShadowMapUser.Dispose();
    }
}
