using Mini.Engine.Configuration;
using Mini.Engine.ECS.Pipeline;

namespace Mini.Engine
{
    [Service]
    internal sealed class GameLoop : IDisposable
    {
        private readonly ParallelPipeline Pipeline;

        public GameLoop(RenderPipelineBuilder builder) => this.Pipeline = builder.Build();

        public void Draw() => this.Pipeline.Frame();

        public void Dispose() => this.Pipeline.Dispose();
    }
}
