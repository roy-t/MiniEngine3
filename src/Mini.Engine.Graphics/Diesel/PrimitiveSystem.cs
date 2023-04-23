﻿using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Diesel;
[Service]
public sealed class PrimitiveSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly PrimitiveRenderService RenderService;
    private readonly CameraService CameraService;

    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public PrimitiveSystem(Device device, PrimitiveRenderService renderService, CameraService cameraService, IComponentContainer<PrimitiveComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<PrimitiveSystem>();
        this.RenderService = renderService;
        this.CameraService = cameraService;
        this.Primitives = models;
        this.Transforms = transforms;
    }

    public Task<CommandList> DrawPrimitives(RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int heigth, float alpha)
    {
        return Task.Run(() =>
        {
            this.RenderService.Setup(this.Context, albedo, depth, x, y, width, heigth);

            ref var camera = ref this.CameraService.GetPrimaryCamera();
            ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();

            foreach (ref var primitive in this.Primitives.IterateAll())
            {
                if (this.Transforms.Contains(primitive.Entity))
                {
                    ref var transform = ref this.Transforms[primitive.Entity].Value;
                    this.RenderService.Render(this.Context, in camera.Camera, in cameraTransform.Current, in primitive.Value, in transform.Current);
                }
            }

            return this.Context.FinishCommandList();
        });
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}