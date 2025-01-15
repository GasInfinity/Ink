namespace Ink.Text;

public readonly record struct HoverEvent
{
    public readonly HoverEventAction Action { get; init; }
    public readonly object? Contents { get; init; }
}
