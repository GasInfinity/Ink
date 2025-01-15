namespace Ink.Registries;

public readonly record struct Tag(Identifier Id)
{
    public readonly Identifier Id = Id;

    public override string ToString()
        => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"#{Id}";

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => destination.TryWrite($"#{Id}", out charsWritten);

    public static implicit operator Tag(Identifier id)
        => new(id);
}
