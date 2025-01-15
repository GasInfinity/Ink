namespace Ink.Text;

public record TextStyleBuilder(bool? Bold = null, bool? Italic = null, bool? Underlined = null, bool? Strikethrough = null, bool? Obfucated = null, TextColor? Color = null, string? Font = null, string? Insertion = null, ClickEvent? ClickEvent = null, HoverEvent? HoverEvent = null)
{
    public static TextStyle Empty
        => new();

    private bool? bold = Bold;
    private bool? italic = Italic;
    private bool? underlined = Underlined;
    private bool? strikethrough = Strikethrough;
    private bool? obfuscated = Obfucated;
    private TextColor? color = Color;
    private string? font = Font;
    private string? insertion = Insertion;
    private ClickEvent? clickEvent = ClickEvent;
    private HoverEvent? hoverEvent = HoverEvent;

    public TextStyleBuilder Boldi(bool bold)
    {
        this.bold = bold;
        return this;
    }

    public TextStyle ToTextStyle()
        => new (bold, italic, underlined, strikethrough, obfuscated, color, font, insertion, clickEvent, hoverEvent);
}
