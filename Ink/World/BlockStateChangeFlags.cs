namespace Ink.World;

[Flags]
public enum BlockStateChangeFlags
{
    None,
    NotifyListeners = 1 << 0,
    NotifyNeighbors = 1 << 1,
    SkipDrops = 1 << 2,
    Moved = 1 << 3,
    NotifyAll = NotifyListeners | NotifyNeighbors
}
