using System;
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
}