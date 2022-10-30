using Serilog;

namespace Mini.Engine.Core.Lifetime;

public sealed class LifetimeManager : IDisposable
{
    private readonly ILogger Logger;
    private readonly StackPool Pool;
    private readonly Stack<string> Names;

    public LifetimeManager(ILogger logger)
    {
        this.Logger = logger.ForContext<LifetimeManager>();
        this.Pool = new StackPool();
        this.Names = new Stack<string>();
    }

    public ILifetime<T> Add<T>(T disposable)
        where T : IDisposable
    {
        if (this.Names.Count == 0)
        {
            throw new InvalidOperationException("Push a frame before trying to something");
        }
        return this.Pool.Add(disposable, this.Names.Count);
    }

    public T Get<T>(ILifetime<T> lifetime)
        where T : IDisposable
    {
        return (T)this.Pool[lifetime];
    }

    public void PushFrame(string name)
    {
        this.Logger.Information("Pushing lifetime frame {@frame}", name);
        this.Names.Push(name);
    }

    public void PopFrame()
    {
        var index = this.Names.Count;
        var name = this.Names.Pop();
        this.Logger.Information("Disposing lifetime frame {@frame}", name);

        this.Pool.DisposeAll(index);
    }

    public void Dispose()
    {
        while (this.Names.Count > 0)
        {
            this.PopFrame();
        }
    }
}
