using System.Security.AccessControl;
using Serilog;

namespace Mini.Engine.Core.Lifetime;

public sealed class LifetimeManager : IDisposable
{
    private int version = 0;

    private record Frame(string Name, int Version);

    private readonly ILogger Logger;
    private readonly StackPool Pool;
    private readonly Stack<Frame> Frames;

    public LifetimeManager(ILogger logger)
    {
        this.Logger = logger.ForContext<LifetimeManager>();
        this.Pool = new StackPool();
        this.Frames = new Stack<Frame>();
    }

    public ILifetime<T> Add<T>(T disposable)
        where T : IDisposable
    {
        if (this.Frames.Count == 0)
        {
            throw new InvalidOperationException("Push a frame before trying to something");
        }
        return this.Pool.Add(disposable, this.version);
    }

    public T Get<T>(ILifetime<T> lifetime)
        where T : IDisposable
    {
        return (T)this.Pool[lifetime];
    }

    public void PushFrame(string name)
    {
        this.Logger.Information("Pushing lifetime frame {@frame}", name);
        this.version++;
        this.Frames.Push(new Frame(name, this.version));
    }

    public void PopFrame()
    {        
        var frame = this.Frames.Pop();
        this.Logger.Information("Disposing lifetime frame {@frame} v{@index}", frame.Name, frame.Version);

        this.Pool.DisposeAll(frame.Version);
    }

    public void Dispose()
    {
        while (this.Frames.Count > 0)
        {
            this.PopFrame();
        }
    }
}
