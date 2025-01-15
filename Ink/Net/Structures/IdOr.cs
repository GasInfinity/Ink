namespace Ink.Net.Structures;

public readonly struct IdOr<T>(int Id, T Value)
    where T : struct
{
    public readonly int Id = Id;
    public readonly T Value = Value;

    public bool HasValue
        => Id == default;

    public IdOr(int id) : this(id, default)
    { }

    public IdOr(T value) : this(0, value)
    { }
}
