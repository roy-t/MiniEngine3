using Mini.Engine.Configuration;
using Mini.Engine.ECS.Pipeline;

namespace Mini.Engine
{
    [Service]
    internal sealed class GameLoop
    {
        private readonly ParallelPipeline Pipeline;

        public GameLoop(RenderPipelineBuilder builder)
        {
            this.Pipeline = builder.Build();
        }

        public void Draw(float elapsed)
        {
            this.Pipeline.Frame();
        }

        internal void Initialize()
        {
        }

    }
}
