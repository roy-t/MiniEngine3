using System.Runtime.CompilerServices;
using Serilog;

namespace Mini.Engine.Core.Lifetime;


public record LifeTimeFrame(int Id);

public sealed class LifetimeManager : IDisposable
{
    private int id = 0;

    private readonly ILogger Logger;
    private readonly VersionedPool Pool;

    public LifetimeManager(ILogger logger)
    {
        this.Logger = logger.ForContext<LifetimeManager>();
        this.Pool = new VersionedPool();
    }

    public ILifetime<T> Add<T>(T disposable)
        where T : IDisposable
    {
        if (this.id == 0)
        {
            throw new InvalidOperationException("Push a frame before trying to add something");
        }
        return this.Pool.Add(disposable, this.id);
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

    public LifeTimeFrame PushFrame([CallerFilePath] string caller = "", [CallerLineNumber] int line = 0)
    {
        this.Logger.Information("Pushing lifetime frame {@caller}:{@line}", caller, line);
        this.id++;
        return new LifeTimeFrame(this.id);
    }

    public void PopFrame(LifeTimeFrame frame, [CallerFilePath] string caller = "", [CallerLineNumber] int line = 0)
    {
        this.Logger.Information("Disposing lifetime frame {@caller}:{@line}", caller, line);
        this.Pool.DisposeAll(frame.Id);
    }

    public void Dispose()
    {
        if (this.Pool.Count > 0)
        {
            throw new Exception("All frames should have been disposed before LifetimeManager is disposed");
        }
    }
}
