namespace Ink.Util;

public record struct ThreadSafeFlag
{
    const int NotSet = 0;
    const int Set = 1;

    private int flag;

    public readonly bool IsSet
        => flag == Set;
    
    public bool TrySet()
        => Interlocked.CompareExchange(ref flag, Set, NotSet) == NotSet;

    public void Reset()
        => Interlocked.Exchange(ref flag, NotSet);
}
