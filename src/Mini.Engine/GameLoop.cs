using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS.Pipeline;

namespace Mini.Engine
{
    [Service]
    internal sealed class GameLoop : IDisposable
    {
        private readonly ContentManager Content;
        private readonly ParallelPipeline Pipeline;

        public GameLoop(RenderPipelineBuilder builder, ContentManager content)
        {
            this.Content = content;
            this.Pipeline = builder.Build();
        }

        public void Update()
        {
            this.Content.ReloadChangedContent();
        }

        public void Draw()
        {
            this.Pipeline.Frame();
        }

        public void Dispose()
        {
            this.Pipeline.Dispose();
        }
    }
}
