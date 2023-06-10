using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D11;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed class CascadedShadowMapSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly ModelRenderService ModelRenderService;
    private readonly PrimitiveRenderService PrimitiveRenderService;
    private readonly LightFrustum Frustum;

    private readonly IComponentContainer<CascadedShadowMapComponent> ShadowMaps;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public CascadedShadowMapSystem(Device device, FrameService frameService, ModelRenderService modelRenderService, PrimitiveRenderService primitiveRenderService, IComponentContainer<CascadedShadowMapComponent> shadowMaps, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<CascadedShadowMapSystem>();
        this.FrameService = frameService;
        this.ModelRenderService = modelRenderService;
        this.PrimitiveRenderService = primitiveRenderService;
        this.Frustum = new LightFrustum();
        this.ShadowMaps = shadowMaps;
        this.Transforms = transforms;
    }

    public Task<CommandList> Render(float alpha)
    {
        return Task.Run(() =>
        {
            foreach (ref var component in this.ShadowMaps.IterateAll())
            {
                var entity = component.Entity;
                if (entity.HasComponent(this.Transforms))
                {
                    ref var shadowMap = ref component.Value;
                    ref var transform = ref this.Transforms[component.Entity].Value;
                    this.DrawCascades(in shadowMap, in transform);
                }
            }

            return this.Context.FinishCommandList();
        });
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

    private void UpdateCascades(ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        var surfaceToLight = -viewPoint.Current.GetForward();
        shadowMap.GlobalShadowMatrix = CreateGlobalShadowMatrix(surfaceToLight, this.Frustum);

        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var clipDistance = camera.FarPlane - camera.NearPlane;

        shadowMap.Cascades.X = 0.075f;
        shadowMap.Cascades.Y = 0.15f;
        shadowMap.Cascades.Z = 0.30f;
        shadowMap.Cascades.W = 1.00f;

        (var s0, var o0, var x0) = this.UpdateShadowMap(ref shadowMap, 0.0f, shadowMap.Cascades.X, clipDistance, in camera, in cameraTransform, surfaceToLight);
        (var s1, var o1, var x1) = this.UpdateShadowMap(ref shadowMap, shadowMap.Cascades.X, shadowMap.Cascades.Y, clipDistance, in camera, in cameraTransform, surfaceToLight);
        (var s2, var o2, var x2) = this.UpdateShadowMap(ref shadowMap, shadowMap.Cascades.Y, shadowMap.Cascades.Z, clipDistance, in camera, in cameraTransform, surfaceToLight);
        (var s3, var o3, var x3) = this.UpdateShadowMap(ref shadowMap, shadowMap.Cascades.Z, shadowMap.Cascades.W, clipDistance, in camera, in cameraTransform, surfaceToLight);

        shadowMap.Splits.X = s0;
        shadowMap.Splits.Y = s1;
        shadowMap.Splits.Z = s2;
        shadowMap.Splits.W = s3;

        shadowMap.Offsets = Matrices.CreateColumnMajor(o0, o1, o2, o3);
        shadowMap.Scales = Matrices.CreateColumnMajor(x0, x1, x2, x3);
    }

    private void DrawCascades(in CascadedShadowMapComponent shadowMap, in TransformComponent viewPoint)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;
        var surfaceToLight = -viewPoint.Current.GetForward();

        this.Frustum.TransformToCameraFrustumInWorldSpace(in camera, in cameraTransform);

        this.RenderShadowMap(in shadowMap, 0.0f, shadowMap.Cascades.X, in camera, in cameraTransform, surfaceToLight, 0);
        this.RenderShadowMap(in shadowMap, shadowMap.Cascades.X, shadowMap.Cascades.Y, in camera, in cameraTransform, surfaceToLight, 1);
        this.RenderShadowMap(in shadowMap, shadowMap.Cascades.Y, shadowMap.Cascades.Z, in camera, in cameraTransform, surfaceToLight, 2);
        this.RenderShadowMap(in shadowMap, shadowMap.Cascades.Z, shadowMap.Cascades.W, in camera, in cameraTransform, surfaceToLight, 3);
    }

    private (float split, Vector4 offset, Vector4 scale) UpdateShadowMap(ref CascadedShadowMapComponent shadowMap, float nearZ, float farZ, float clipDistance, in PerspectiveCamera view, in Transform viewTransform, Vector3 surfaceToLight)
    {
        this.Frustum.TransformToCameraFrustumInWorldSpace(in view, in viewTransform);
        this.Frustum.Slice(nearZ, farZ);

        var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
        var shadowMatrix = CreateSliceShadowMatrix(viewProjection);

        var nearCorner = TransformCorner(Vector3.Zero, shadowMatrix, shadowMap.GlobalShadowMatrix);
        var farCorner = TransformCorner(Vector3.One, shadowMatrix, shadowMap.GlobalShadowMatrix);

        return (view.NearPlane + (farZ * clipDistance), new Vector4(-nearCorner, 0.0f), new Vector4(Vector3.One / (farCorner - nearCorner), 1.0f));
    }

    private void RenderShadowMap(in CascadedShadowMapComponent shadowMap, float nearZ, float farZ, in PerspectiveCamera view, in Transform viewTransform, Vector3 surfaceToLight, int slice)
    {
        this.Frustum.TransformToCameraFrustumInWorldSpace(in view, in viewTransform);
        this.Frustum.Slice(nearZ, farZ);

        var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);

        this.Context.OM.SetRenderTarget(shadowMap.DepthBuffers, slice);
        this.Context.Clear(shadowMap.DepthBuffers, slice, DepthStencilClearFlags.Depth, 1.0f, 0);

        var viewVolume = new Frustum(viewProjection);
        var output = new Rectangle(0, 0, shadowMap.Resolution, shadowMap.Resolution);

        this.ModelRenderService.SetupAndRenderAllModelDepths(this.Context, float.MinValue, in output, in output, in viewVolume, in viewProjection);
        this.PrimitiveRenderService.SetupAndRenderAllPrimitiveDepths(this.Context, float.MinValue, in output, in output, in viewVolume, in viewProjection);
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

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
