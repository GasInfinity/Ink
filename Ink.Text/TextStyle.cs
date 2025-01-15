namespace Ink.Text;

public record TextStyle(bool? Bold = null, bool? Italic = null, bool? Underlined = null, bool? Strikethrough = null, bool? Obfucated = null, TextColor? Color = null, string? Font = null, string? Insertion = null, ClickEvent? ClickEvent = null, HoverEvent? HoverEvent = null)
{
    public static TextStyle Empty
        => new();

    public readonly bool? Bold = Bold;
    public readonly bool? Italic = Italic;
    public readonly bool? Underlined = Underlined;
    public readonly bool? Strikethrough = Strikethrough;
    public readonly bool? Obfuscated = Obfucated;
    public readonly TextColor? Color = Color;
    public readonly string? Font = Font;
    public readonly string? Insertion = Insertion;
    public readonly ClickEvent? ClickEvent = ClickEvent;
    public readonly HoverEvent? HoverEvent = HoverEvent;

    public bool IsEmpty
        => !Bold.HasValue 
        && !Italic.HasValue 
        && !Underlined.HasValue 
        && !Strikethrough.HasValue 
        && !Obfuscated.HasValue 
        && !Color.HasValue 
        && Font == null 
        && Insertion == null 
        && !ClickEvent.HasValue 
        && !HoverEvent.HasValue;
}
