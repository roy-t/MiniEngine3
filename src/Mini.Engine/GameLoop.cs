using System;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
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
        private readonly FrameService FrameService;
        private readonly ContentManager Content;
        private readonly ParallelPipeline Pipeline;

        public GameLoop(Device device, FrameService frameService, RenderPipelineBuilder builder, ContentManager content, EntityAdministrator entities, IComponentContainer<ModelComponent> models)
        {
            this.FrameService = frameService;
            this.Content = content;
            this.Pipeline = builder.Build();

            // TODO: move to scene
            var entity = entities.Create();
            models.Add(new ModelComponent(entity, new Model(device, new ModelData())));
        }

        public void Update(float time, float elapsed)
        {
            this.Content.ReloadChangedContent();
        }

        public void Draw(float alpha)
        {
            this.FrameService.Alpha = alpha;
            this.Pipeline.Frame();
            // TODO: post process and render to screen
        }

        public void Dispose()
        {
            this.Pipeline.Dispose();
        }
    }
}
