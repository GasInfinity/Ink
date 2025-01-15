namespace Ink.Text;

public readonly record struct ClickEvent
{
    public readonly ClickEventAction Action { get; init; }
    public readonly string Value { get; init; }
}
