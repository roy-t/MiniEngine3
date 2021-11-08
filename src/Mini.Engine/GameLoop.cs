using System;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Controllers;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;

namespace Mini.Engine
{
    [Service]
    internal sealed class GameLoop : IDisposable
    {
        private readonly Device Device;
        private readonly RenderHelper Helper;
        private readonly FrameService FrameService;
        private readonly CameraController CameraController;
        private readonly ContentManager Content;
        private readonly ParallelPipeline Pipeline;

        public GameLoop(Device device, RenderHelper helper, FrameService frameService, CameraController cameraController, RenderPipelineBuilder builder, ContentManager content, EntityAdministrator entities, IComponentContainer<ModelComponent> models)
        {
            this.Device = device;
            this.Helper = helper;
            this.FrameService = frameService;
            this.CameraController = cameraController;
            this.Content = content;
            this.Pipeline = builder.Build();

            // TODO: move to scene
            var entity = entities.Create();
            models.Add(new ModelComponent(entity, new Model(device, content.FileSystem, string.Empty)));
        }

        public void Update(float time, float elapsed)
        {
            this.Content.ReloadChangedContent();
            this.CameraController.Update(this.FrameService.Camera, elapsed);
        }

        public void Draw(float alpha)
        {
            this.FrameService.Alpha = alpha;
            this.Pipeline.Frame();

            this.Helper.RenderToViewPort(this.Device.ImmediateContext, this.FrameService.GBuffer.Albedo);
        }

        public void Dispose()
        {
            this.Pipeline.Dispose();
        }
    }
}
