using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.v2;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class CascadedShadowMapSystem : IModelRenderCallBack, ISystem, IDisposable
{
    // TODO: this system still uses default, instead of reverse-z. Fix?


    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly RenderService RenderService;
    private readonly ShadowMap Shader;
    private readonly ShadowMap.User User;
    private readonly InputLayout InputLayout;
    private readonly LightFrustum Frustum;
    private readonly IMaterial DefaultMaterial;

    public CascadedShadowMapSystem(Device device, FrameService frameService, RenderService renderService, ShadowMap shader, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<CascadedShadowMapSystem>();
        this.FrameService = frameService;
        this.RenderService = renderService;
        this.Shader = shader;
        this.User = shader.CreateUserFor<CascadedShadowMapSystem>();

        this.InputLayout = this.Shader.CreateInputLayoutForVs(ModelVertex.Elements);
        this.Frustum = new LightFrustum();
        
        this.DefaultMaterial = content.LoadDefaultMaterial();
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.Device.RasterizerStates.CullNoneNoDepthClip, 0, 0, 1024, 1024, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.Default);
        this.Context.VS.SetConstantBuffer(ShadowMap.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetSampler(ShadowMap.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawCascades(ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform().Transform;
        var surfaceToLight = -viewPoint.Transform.GetForward();

        this.Frustum.TransformToCameraFrustumInWorldSpace(in camera, in cameraTransform);

        shadowMap.GlobalShadowMatrix = CreateGlobalShadowMatrix(surfaceToLight, this.Frustum);

        var totalViewProjectioNMatrix = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var viewVolume = new Frustum(totalViewProjectioNMatrix);

        var clipDistance = camera.FarPlane - camera.NearPlane;

        (var s0, var o0, var x0) = this.RenderShadowMap(ref shadowMap, 0.0f, shadowMap.Cascades.X, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 0);
        (var s1, var o1, var x1) = this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.X, shadowMap.Cascades.Y, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 1);
        (var s2, var o2, var x2) = this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.Y, shadowMap.Cascades.Z, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 2);
        (var s3, var o3, var x3) = this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.Z, shadowMap.Cascades.W, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 3);

        shadowMap.Splits.X = s0;
        shadowMap.Splits.Y = s1;
        shadowMap.Splits.Z = s2;
        shadowMap.Splits.W = s3;

        shadowMap.Offsets = Matrices.CreateColumnMajor(o0, o1, o2, o3);
        shadowMap.Scales = Matrices.CreateColumnMajor(x0, x1, x2, x3);
    }

    private (float split, Vector4 offset, Vector4 scale) RenderShadowMap(ref CascadedShadowMapComponent shadowMap, float nearZ, float farZ, float clipDistance, PerspectiveCamera view, Transform viewTransform, Vector3 surfaceToLight, Frustum viewVolume, int slice)
    {
        this.Frustum.TransformToCameraFrustumInWorldSpace(in view, in viewTransform);
        this.Frustum.Slice(nearZ, farZ);

        var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var shadowMatrix = CreateSliceShadowMatrix(viewProjection);
        
        this.RenderShadowMap(shadowMap.DepthBuffers, shadowMap.Resolution, slice, viewProjection);

        var nearCorner = TransformCorner(Vector3.Zero, shadowMatrix, shadowMap.GlobalShadowMatrix);
        var farCorner = TransformCorner(Vector3.One, shadowMatrix, shadowMap.GlobalShadowMatrix);

        return (view.NearPlane + (farZ * clipDistance), new Vector4(-nearCorner, 0.0f), new Vector4(Vector3.One / (farCorner - nearCorner), 1.0f));        
    }

    private void RenderShadowMap(ILifetime<IDepthStencilBuffer> depthStencilBuffers, int resolution, int slice, Matrix4x4 viewProjection)
    {
        this.Context.RS.SetViewPort(0, 0, resolution, resolution);
        this.Context.RS.SetScissorRect(0, 0, resolution, resolution);
        this.Context.OM.SetRenderTarget(depthStencilBuffers, slice);

        this.Context.Clear(depthStencilBuffers, slice, DepthStencilClearFlags.Depth, 1.0f, 0);

        this.RenderService.DrawAllModels(this, this.Context, viewProjection);

        this.Context.PS.SetShaderResource(ShadowMap.Albedo, this.DefaultMaterial.Albedo);
        this.RenderService.DrawAllTerrain(this, this.Context, viewProjection);
    }

    public void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world)
    {
        this.User.MapConstants(this.Context, worldViewProjection);
    }

    public void SetMaterial(IMaterial material)
    {
        this.Context.PS.SetShaderResource(ShadowMap.Albedo, material.Albedo);
    }

    private static readonly Matrix4x4 TexScaleTransform = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.0f);

    private static Matrix4x4 CreateGlobalShadowMatrix(Vector3 surfaceToLight, LightFrustum frustum)
    {
        var frustumCenter = frustum.ComputeCenter();
        var shadowCameraPos = frustumCenter + (surfaceToLight * -0.5f);

        var view = Matrix4x4.CreateLookAt(shadowCameraPos, frustumCenter, Vector3.UnitY);
        var projection = Matrix4x4.CreateOrthographicOffCenter(-0.5f, 0.5f, -0.5f, 0.5f, 0.0f, 1.0f);

        return view * projection * TexScaleTransform;
    }

    private static Matrix4x4 ComputeViewProjectionMatrixForSlice(Vector3 surfaceToLight, LightFrustum frustum, float resolution)
    {
        var bounds = frustum.ComputeBounds();
        var radius = (float)Math.Ceiling(bounds.Radius);

        var position = bounds.Center + (surfaceToLight * radius);

        var view = Matrix4x4.CreateLookAt(position, bounds.Center, Vector3.UnitY);
        var projection = Matrix4x4.CreateOrthographicOffCenter(
            -radius,
            radius,
            -radius,
            radius,
            0.0f,
            radius * 2);

        var origin = Vector3.Transform(Vector3.Zero, view * projection) * (resolution / 2.0f);

        var roundedOrigin = Round(origin);
        var roundOffset = (roundedOrigin - origin) * (2.0f / resolution);

        projection.M41 += roundOffset.X;
        projection.M42 += roundOffset.Y;

        return view * projection;
    }

    private static Matrix4x4 CreateSliceShadowMatrix(Matrix4x4 sliceViewProjection)
    {
        return sliceViewProjection * TexScaleTransform;
    }

    private static Vector3 Round(Vector3 value)
    {
        return new Vector3(MathF.Round(value.X), MathF.Round(value.Y), MathF.Round(value.Z));
    }

    private static Vector3 TransformCorner(Vector3 corner, Matrix4x4 shadowMatrix, Matrix4x4 globalShadowMatrix)
    {
        Matrix4x4.Invert(shadowMatrix, out var inv);
        var v = ScaleToVector3(Vector4.Transform(corner, inv));
        return ScaleToVector3(Vector4.Transform(v, globalShadowMatrix));
    }

    private static Vector3 ScaleToVector3(Vector4 value)
    {
        return new Vector3(value.X, value.Y, value.Z) / value.W;
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.InputLayout.Dispose();
        this.Context.Dispose();
        this.User.Dispose();
    }
}
