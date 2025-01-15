using System.Collections.Concurrent;

namespace Ink.Util;

public record struct Scheduler : IAsyncTickable
{
    private readonly ConcurrentQueue<Action> scheduledActions = new();
    private ThreadSafeFlag tickingFlag = new();

    public Scheduler()
    {
    }

    public readonly void Schedule(Action action)
        => scheduledActions.Enqueue(action);

    public ValueTask TickAsync()
    {
        if (!this.tickingFlag.TrySet())
            ThrowHelpers.ThrowTickingWhileTicked();

        try
        {
            while (this.scheduledActions.TryDequeue(out Action? action))
                action.Invoke();
        }
        finally
        {
            this.tickingFlag.Reset();
        }

        return ValueTask.CompletedTask;
    }
}
