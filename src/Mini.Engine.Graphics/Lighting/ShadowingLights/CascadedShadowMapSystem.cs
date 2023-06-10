using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D11;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class CascadedShadowMapSystem : ISystem, IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly ModelRenderService ModelRenderService;
    private readonly LightFrustum Frustum;

    private readonly IComponentContainer<CascadedShadowMapComponent> ShadowMaps;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public CascadedShadowMapSystem(Device device, FrameService frameService, ModelRenderService modelRenderService, IComponentContainer<CascadedShadowMapComponent> shadowMaps, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<CascadedShadowMapSystem>();
        this.FrameService = frameService;
        this.ModelRenderService = modelRenderService;

        this.Frustum = new LightFrustum();
        this.ShadowMaps = shadowMaps;
        this.Transforms = transforms;
    }


    public Task<CommandList> Render(float alpha)
    {
        return Task.Run(() =>
        {            
            foreach (ref var shadowMap in this.ShadowMaps.IterateAll())
            {
                if (this.Transforms.Contains(shadowMap.Entity))
                {
                    ref var transform = ref this.Transforms[shadowMap.Entity];
                    this.DrawCascades(ref shadowMap.Value, ref transform.Value);
                }
            }

            return this.Context.FinishCommandList();
        });
    }


    public void OnSet()
    {
    }

    public void Update()
    {
        foreach (ref var shadowMap in this.ShadowMaps.IterateAll())
        {
            if (this.Transforms.Contains(shadowMap.Entity))
            {
                ref var transform = ref this.Transforms[shadowMap.Entity];
                this.UpdateCascades(ref shadowMap.Value, ref transform.Value);
            }
        }
    }

    public void UpdateCascades(ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        var surfaceToLight = -viewPoint.Current.GetForward();
        shadowMap.GlobalShadowMatrix = CreateGlobalShadowMatrix(surfaceToLight, this.Frustum);

        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var totalViewProjectioNMatrix = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var viewVolume = new Frustum(totalViewProjectioNMatrix);

        var clipDistance = camera.FarPlane - camera.NearPlane;

        (var s0, var o0, var x0) = this.DoNot(ref shadowMap, 0.0f, shadowMap.Cascades.X, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 0);
        (var s1, var o1, var x1) = this.DoNot(ref shadowMap, shadowMap.Cascades.X, shadowMap.Cascades.Y, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 1);
        (var s2, var o2, var x2) = this.DoNot(ref shadowMap, shadowMap.Cascades.Y, shadowMap.Cascades.Z, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 2);
        (var s3, var o3, var x3) = this.DoNot(ref shadowMap, shadowMap.Cascades.Z, shadowMap.Cascades.W, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 3);

        shadowMap.Splits.X = s0;
        shadowMap.Splits.Y = s1;
        shadowMap.Splits.Z = s2;
        shadowMap.Splits.W = s3;

        shadowMap.Offsets = Matrices.CreateColumnMajor(o0, o1, o2, o3);
        shadowMap.Scales = Matrices.CreateColumnMajor(x0, x1, x2, x3);
    }

    private (float split, Vector4 offset, Vector4 scale) DoNot(ref CascadedShadowMapComponent shadowMap, float nearZ, float farZ, float clipDistance, PerspectiveCamera view, Transform viewTransform, Vector3 surfaceToLight, Frustum viewVolume, int slice)
    {
        this.Frustum.TransformToCameraFrustumInWorldSpace(in view, in viewTransform);
        this.Frustum.Slice(nearZ, farZ);

        var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var shadowMatrix = CreateSliceShadowMatrix(viewProjection);

        var nearCorner = TransformCorner(Vector3.Zero, shadowMatrix, shadowMap.GlobalShadowMatrix);
        var farCorner = TransformCorner(Vector3.One, shadowMatrix, shadowMap.GlobalShadowMatrix);

        return (view.NearPlane + (farZ * clipDistance), new Vector4(-nearCorner, 0.0f), new Vector4(Vector3.One / (farCorner - nearCorner), 1.0f));
    }


    [Process(Query = ProcessQuery.All)]
    public void DrawCascades(ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;
        var surfaceToLight = -viewPoint.Current.GetForward();

        this.Frustum.TransformToCameraFrustumInWorldSpace(in camera, in cameraTransform);

        //shadowMap.GlobalShadowMatrix = CreateGlobalShadowMatrix(surfaceToLight, this.Frustum);

        var totalViewProjectioNMatrix = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var viewVolume = new Frustum(totalViewProjectioNMatrix);

        var clipDistance = camera.FarPlane - camera.NearPlane;

        /*(var s0, var o0, var x0) = */this.RenderShadowMap(ref shadowMap, 0.0f, shadowMap.Cascades.X, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 0);
        /*(var s1, var o1, var x1) = */this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.X, shadowMap.Cascades.Y, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 1);
        /*(var s2, var o2, var x2) = */this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.Y, shadowMap.Cascades.Z, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 2);
        /*(var s3, var o3, var x3) = */this.RenderShadowMap(ref shadowMap, shadowMap.Cascades.Z, shadowMap.Cascades.W, clipDistance, camera, cameraTransform, surfaceToLight, viewVolume, 3);

        //shadowMap.Splits.X = s0;
        //shadowMap.Splits.Y = s1;
        //shadowMap.Splits.Z = s2;
        //shadowMap.Splits.W = s3;

        //shadowMap.Offsets = Matrices.CreateColumnMajor(o0, o1, o2, o3);
        //shadowMap.Scales = Matrices.CreateColumnMajor(x0, x1, x2, x3);
    }

    private (float split, Vector4 offset, Vector4 scale) RenderShadowMap(ref CascadedShadowMapComponent shadowMap, float nearZ, float farZ, float clipDistance, PerspectiveCamera view, Transform viewTransform, Vector3 surfaceToLight, Frustum viewVolume, int slice)
    {
        this.Frustum.TransformToCameraFrustumInWorldSpace(in view, in viewTransform);
        this.Frustum.Slice(nearZ, farZ);

        var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var shadowMatrix = CreateSliceShadowMatrix(viewProjection);

        this.RenderShadowMap(shadowMap.DepthBuffers, shadowMap.Resolution, slice, viewProjection, viewProjection);

        var nearCorner = TransformCorner(Vector3.Zero, shadowMatrix, shadowMap.GlobalShadowMatrix);
        var farCorner = TransformCorner(Vector3.One, shadowMatrix, shadowMap.GlobalShadowMatrix);

        return (view.NearPlane + (farZ * clipDistance), new Vector4(-nearCorner, 0.0f), new Vector4(Vector3.One / (farCorner - nearCorner), 1.0f));
    }

    private void RenderShadowMap(ILifetime<IDepthStencilBuffer> depthStencilBuffers, int resolution, int slice, Matrix4x4 previousViewProjection, Matrix4x4 viewProjection)
    {
        this.Context.OM.SetRenderTarget(depthStencilBuffers, slice);
        this.Context.Clear(depthStencilBuffers, slice, DepthStencilClearFlags.Depth, 1.0f, 0);

        var viewVolume = new Frustum(viewProjection);
        var output = new Rectangle(0, 0, resolution, resolution);

        this.ModelRenderService.SetupAndRenderAllModelDepths(this.Context, in output, in output, in viewVolume, in viewProjection);
        //this.TerrainRenderService.SetupAndRenderAllTerrainDepths(this.Context, in output, in output, in viewVolume, in viewProjection);
        //this.TileRenderService.SetupAndRenderAllTileDepths(this.Context, in output, in output, in viewVolume, in viewProjection);
        //this.TileRenderService.SetupAndRenderAllTileWallDepths(this.Context, in output, in output, in viewVolume, in viewProjection);
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
        //using var commandList = this.Context.FinishCommandList();
        //this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
