using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.PostProcessing;

[Service]
public sealed partial class PostProcessingSystem : ISystem, IDisposable
{
    public void OnSet()
    {

    }

    [Process(Query = ProcessQuery.None)]
    public void PostProcess()
    {
        // TODO: see GameLoop.Draw()
        // - add post process buffer to frame service
        // - draw and FXAA eveyrthing to post process buffer
        // - swap
        // then present the PP buffer in Draw
    }

    public void OnUnSet()
    {

    }

    public void Dispose()
    {

    }
}
