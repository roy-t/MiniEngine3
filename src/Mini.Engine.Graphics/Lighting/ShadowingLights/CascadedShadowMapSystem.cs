using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.ShadowMap;
using Vortice.Direct3D;
using Mini.Engine.ECS.Generators.Shared;
using System.Numerics;
using Mini.Engine.Graphics.Models;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class CascadedShadowMapSystem : IRenderServiceCallBack, ISystem, IDisposable
{
    private readonly Device Device;
    private readonly ImmediateDeviceContext Context;    
    private readonly FrameService FrameService;
    private readonly RenderService RenderService;
    private readonly ShadowMapVs VertexShader;
    private readonly ShadowMapPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;


    private readonly LightFrustum Frustum;

    public CascadedShadowMapSystem(Device device, FrameService frameService, RenderService renderService, ShadowMapVs vertexShader, ShadowMapPs pixelShader)
    {
        this.Device = device;
        this.Context = device.ImmediateContext;
        this.FrameService = frameService;
        this.RenderService = renderService;
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;

        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(CascadedShadowMapSystem)}_CB");

        this.Frustum = new LightFrustum();
    }    

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.VertexShader, this.Device.RasterizerStates.CullNoneNoDepthClip, 0, 0, 1024, 1024, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.Default);
        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
        this.Context.PS.SetSampler(ShadowMap.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawCascades(CascadedShadowMapComponent shadowMap, TransformComponent viewPoint)
    {
        var view = this.FrameService.Camera;
        var surfaceToLight = -viewPoint.Transform.Forward;

        this.Frustum.TransformToCameraFrustumInWorldSpace(view);

        shadowMap.GlobalShadowMatrix = CreateGlobalShadowMatrix(surfaceToLight, this.Frustum);

        for (var i = 0; i < shadowMap.Cascades.Length; i++)
        {
            this.Frustum.TransformToCameraFrustumInWorldSpace(view);

            var nearZ = i == 0 ? 0.0f : shadowMap.Cascades[i - 1];
            var farZ = shadowMap.Cascades[i];
            this.Frustum.Slice(nearZ, farZ);

            var viewProjection = ComputeViewProjectionMatrixForSlice(surfaceToLight, this.Frustum, shadowMap.Resolution);
            var shadowMatrix = CreateSliceShadowMatrix(viewProjection);

            var clipDistance = view.FarPlane - view.NearPlane;
            shadowMap.Splits[i] = view.NearPlane + (farZ * clipDistance);

            var nearCorner = TransformCorner(Vector3.Zero, shadowMatrix, shadowMap.GlobalShadowMatrix);
            var farCorner = TransformCorner(Vector3.One, shadowMatrix, shadowMap.GlobalShadowMatrix);

            shadowMap.Offsets[i] = new Vector4(-nearCorner, 0.0f);
            shadowMap.Scales[i] = new Vector4(Vector3.One / (farCorner - nearCorner), 1.0f);

            this.RenderShadowMap(shadowMap.RenderTargets[i], shadowMap.DepthBuffers[i], viewProjection);
        }
    }

    private void RenderShadowMap(RenderTarget2D shadowMap, DepthStencilBuffer depthStencilBuffer, Matrix4x4 viewProjection)
    {
        this.Context.RS.SetViewPort(0, 0, shadowMap.Width, shadowMap.Height);
        this.Context.RS.SetScissorRect(0, 0, shadowMap.Width, shadowMap.Height);
        this.Context.OM.SetRenderTarget(shadowMap, depthStencilBuffer);
        
        this.Device.Clear(shadowMap, Color4.White);
        this.Device.Clear(depthStencilBuffer, DepthStencilClearFlags.Depth, 1.0f, 0);

        this.RenderService.DrawAllModels(this, this.Context, viewProjection);
    }

    public void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world)
    {
        var cBuffer = new Constants()
        {
            WorldViewProjection = worldViewProjection,
        };
        this.ConstantBuffer.MapData(this.Context, cBuffer);
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

    public void OnUnSet() { }

    public void Dispose()
    {
        this.InputLayout.Dispose();
    }
}
