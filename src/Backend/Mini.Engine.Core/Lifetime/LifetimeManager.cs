namespace Mini.Engine.Core.Lifetime;

public sealed class LifetimeManager
{
    private readonly StackPool Pool;
    private int currentFrame;

    public LifetimeManager()
    {
        this.Pool = new StackPool();
        this.currentFrame = 1;
    }

    public ILifetime<T> Add<T>(T disposable)
        where T : IDisposable
    {
        return this.Pool.Add(disposable, this.currentFrame);
    }

    public T Get<T>(ILifetime<T> lifetime)
        where T : IDisposable
    {
        return (T)this.Pool[lifetime];
    }

    public void PushFrame()
    {
        this.currentFrame += 1;
    }

    public void PopFrame()
    {
        var frame = this.currentFrame;
        this.currentFrame -= 1;

        if (this.currentFrame <= 0)
        {
            throw new Exception("Nothing to pop");
        }

        this.Pool.DisposeAll(frame);
    }
}
