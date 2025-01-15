namespace Ink.Text.Serialization;

public static class SerializationConstants
{
    public static ReadOnlySpan<byte> TextKey => "text"u8;
    public static ReadOnlySpan<byte> TranslateKey => "translate"u8;
    public static ReadOnlySpan<byte> WithKey => "with"u8;
    public static ReadOnlySpan<byte> KeybindKey => "keybind"u8;
    public static ReadOnlySpan<byte> ScoreKey => "score"u8;
    public static ReadOnlySpan<byte> SelectorKey => "selector"u8;
    public static ReadOnlySpan<byte> NbtKey => "nbt"u8; // TODO: NBT
    public static ReadOnlySpan<byte> ExtraKey => "extra"u8;

    public static ReadOnlySpan<byte> BoldKey => "bold"u8;
    public static ReadOnlySpan<byte> ItalicKey => "italic"u8;
    public static ReadOnlySpan<byte> UnderlinedKey => "underlined"u8;
    public static ReadOnlySpan<byte> StrikethroughKey => "strikethrough"u8;
    public static ReadOnlySpan<byte> ObfuscatedKey => "obfuscated"u8;
    public static ReadOnlySpan<byte> ColorKey => "color"u8;
    public static ReadOnlySpan<byte> FontKey => "font"u8;
    public static ReadOnlySpan<byte> InsertionKey => "insertion"u8;
    public static ReadOnlySpan<byte> ClickEventKey => "clickEvent"u8;
    public static ReadOnlySpan<byte> HoverEventKey => "hoverEvent"u8;

    public static ReadOnlySpan<byte> ActionKey => "action"u8;
    public static ReadOnlySpan<byte> ValueKey => "value"u8;
}
