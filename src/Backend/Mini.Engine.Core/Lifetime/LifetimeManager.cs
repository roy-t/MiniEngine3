using Serilog;

namespace Mini.Engine.Core.Lifetime;


public sealed class LifetimeManager : IDisposable
{
    private int version = 0;

    private record Frame(string Name, int Version);

    private readonly ILogger Logger;
    private readonly VersionedPool Pool;
    private readonly Stack<Frame> Frames;

    public LifetimeManager(ILogger logger)
    {
        this.Logger = logger.ForContext<LifetimeManager>();
        this.Pool = new VersionedPool();
        this.Frames = new Stack<Frame>();
    }

    public int FrameCount => this.Frames.Count;

    public ILifetime<T> Add<T>(T disposable)
        where T : IDisposable
    {
        if (this.Frames.Count == 0)
        {
            throw new InvalidOperationException("Push a frame before trying to something");
        }
        return this.Pool.Add(disposable, this.version);
    }

    public bool IsValid<T>(ILifetime<T> target)
        where T : IDisposable
    {
        return this.Pool.IsValid(target);
    }

    public T Get<T>(ILifetime<T> lifetime)
        where T : IDisposable
    {
        return (T)this.Pool[lifetime];
    }

    public void PushFrame(string id)
    {
        this.Logger.Information("Pushing lifetime frame {@frame}", id);
        this.version++;
        this.Frames.Push(new Frame(id, this.version));
    }

    public void PopFrame(string id)
    {
        var frame = this.Frames.Pop();
        if (frame.Name != id)
        {
            throw new Exception($"Unexpected frame, expected: {id} actual: {frame.Name}");
        }
        this.Logger.Information("Disposing lifetime frame {@frame} v{@index}", frame.Name, frame.Version);

        this.Pool.DisposeAll(frame.Version);
    }

    public void Clear()
    {
        while(this.Frames.Count > 0)
        {
            var name = this.Frames.Peek().Name;
            this.PopFrame(name);
        }
    }

    public void Dispose()
    {
        if (this.Frames.Count > 0)
        {
            throw new Exception("All frames should have been disposed before LifetimeManager is disposed");
        }
    }
}
