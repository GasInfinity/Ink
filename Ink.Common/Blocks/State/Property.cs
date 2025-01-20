namespace Ink.Blocks.State;

public abstract record Property
{
    public abstract int PossibleValues { get; }
    public abstract int BitsUsed { get; }

    public abstract bool TryGetIndex<TValue>(TValue value, out int index)
        where TValue : unmanaged;

    public abstract bool TryGetValue<TValue>(int index, out TValue value)
        where TValue : unmanaged;
}
