using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mini.Engine.Core;

public static class BackgroundTasks
{
    public static Task ForAsync(int fromIncluve, int toExclusive, Action<int> body, int maxTasks = 8)
    {
        var length = toExclusive - fromIncluve;
        var taskCount = Math.Min(length, maxTasks);
        var stepLength = length / taskCount;

        var tasks = new Task[taskCount];

        for (var i = 0; i < taskCount; i++)
        {
            var stepFromInclusive = stepLength * i;
            var stepToExclusive = i == (taskCount - 1)
                ? toExclusive
                : stepFromInclusive + stepLength;

            tasks[i] = Task.Run(() =>
            {
                for (var j = stepFromInclusive; j < stepToExclusive; j++)
                {
                    body(j);
                }
            });
            tasks[i].ConfigureAwait(false);
        }

        return Task.WhenAll(tasks);
    }

    public static void For(WorkPool pool, int fromIncluve, int toExclusive, Action<int> body)
    {
        var length = toExclusive - fromIncluve;
        var taskCount = Math.Min(length, pool.Workers);
        var stepLength = length / taskCount;

        for (var i = 0; i < taskCount; i++)
        {
            var stepFromInclusive = stepLength * i;
            var stepToExclusive = i == (taskCount - 1)
                ? toExclusive
                : stepFromInclusive + stepLength;

            pool.AddWork(() =>
            {
                for (var j = stepFromInclusive; j < stepToExclusive; j++)
                {
                    body(j);
                }
            });
        }
    }
}


public sealed class WorkPool : IDisposable
{
    private readonly ConcurrentBag<Action> Work;
    private readonly CancellationTokenSource CancellationToken;
    private readonly Thread[] Threads;

    public WorkPool(int workers)
    {
        this.Work = new ConcurrentBag<Action>();
        this.CancellationToken = new CancellationTokenSource();
        this.Threads = new Thread[workers];

        for (var i = 0; i < this.Threads.Length; i++)
        {
            this.Threads[i] = new Thread(this.Spin) { IsBackground = true };
            this.Threads[i].Start();
        }
    }

    public int Workers => this.Threads.Length;

    public void AddWork(Action action)
    {        
        this.Work.Add(action);        
    }

    public void WaitUntilWorkIsFinished()
    {
        while (!this.CancellationToken.IsCancellationRequested)
        {
            if (this.Work.IsEmpty)
            {
                return;
            }
        }
    }

    private void Spin()
    {
        try
        {
            while (!this.CancellationToken.IsCancellationRequested)
            {
                if (this.Work.TryTake(out var action))
                {
                    action();
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        this.CancellationToken.Cancel();
        for (var i = 0; i < this.Threads.Length; i++)
        {
            this.Threads[i].Join();
        }
    }
}