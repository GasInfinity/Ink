using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ink.Text;

public readonly struct TextColor : IEquatable<TextColor>, ISpanFormattable, IParsable<TextColor>, ISpanParsable<TextColor>
{
    public static readonly TextColor Black = new(0, 0, 0);
    public static readonly TextColor DarkBlue = new(0, 0, 170);
    public static readonly TextColor DarkGreen = new(0, 170, 0);
    public static readonly TextColor DarkAqua = new(0, 170, 170);
    public static readonly TextColor DarkRed = new(170, 0, 0);
    public static readonly TextColor DarkPurple = new(170, 0, 170);
    public static readonly TextColor Gold = new(255, 170, 0);
    public static readonly TextColor Gray = new(170, 170, 170);
    public static readonly TextColor DarkGray = new(85, 85, 58);
    public static readonly TextColor Blue = new(85, 85, 255);
    public static readonly TextColor Green = new(85, 255, 85);
    public static readonly TextColor Aqua = new(85, 255, 255);
    public static readonly TextColor Red = new(255, 85, 85);
    public static readonly TextColor LightPurple = new(255, 85, 255);
    public static readonly TextColor Yellow = new(255, 255, 85);
    public static readonly TextColor White = new(255, 255, 255);

    public readonly byte R;
    public readonly byte G;
    public readonly byte B;

    public TextColor(byte r, byte g, byte b)
        => (R, G, B) = (r, g, b);

    public TextColor(int color)
        => (R, G, B) = ((byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)(color & 0xFF));
    
    public int AsInt()
        => R << 16 | G << 8 | B;

    public bool Equals(TextColor other)
        => R == other.R && G == other.G && B == other.B;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is TextColor other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(R, G, B);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (LegacyColorToString.TryGetValue(this, out var legacyString))
            return destination.TryWrite($"{legacyString}", out charsWritten);

        return destination.TryWrite($"#{R:X2}{G:X2}{B:X2}", out charsWritten);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (LegacyColorToString.TryGetValue(this, out var legacyString))
            return legacyString;

        return $"#{R:X2}{G:X2}{B:X2}";
    }

    public override string ToString()
        => ToString(null, null);

    public static TextColor Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static TextColor Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException($"Error while parsing '{s}', unknown {nameof(TextColor)}");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TextColor result)
        => TryParse(s != null ? s.AsSpan() : [], provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out TextColor result)
    {
        static bool TryParseHtml(ReadOnlySpan<char> s, out TextColor result)
        {
            if (byte.TryParse(s.Slice(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)
            && byte.TryParse(s.Slice(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)
            && byte.TryParse(s.Slice(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
            {
                result = new(r, g, b);
                return true;
            }

            result = default;
            return false;
        }

        if (LegacyStringToColor.TryGetValue(s.ToString(), out var color))
        {
            result = color;
            return true;
        }

        return TryParseHtml(s, out result);
    }

    public static bool operator ==(TextColor left, TextColor right)
        => left.Equals(right);

    public static bool operator !=(TextColor left, TextColor right)
        => !(left == right);

    public static implicit operator int(TextColor color)
        => color.AsInt();
    
    public static implicit operator TextColor(int color)
        => new(color);

    private static readonly FrozenDictionary<int, string> LegacyColorToString = new Dictionary<int, string>()
    {
        { Black, "black" },
        { DarkBlue, "dark_blue" },
        { DarkGreen, "dark_green" },
        { DarkAqua, "dark_aqua" },
        { DarkRed, "dark_red" },
        { DarkPurple, "dark_purple" },
        { Gold, "gold" },
        { Gray, "gray" },
        { DarkGray, "dark_gray" },
        { Blue, "blue" },
        { Green, "green" },
        { Aqua, "aqua" },
        { Red, "red" },
        { LightPurple, "light_purple" },
        { Yellow, "yellow" },
        { White, "white" }
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, TextColor> LegacyStringToColor = new Dictionary<string, TextColor>()
    {
        { "black", Black },
        { "dark_blue", DarkBlue },
        { "dark_green", DarkGreen },
        { "dark_aqua", DarkAqua },
        { "dark_red", DarkRed },
        { "dark_purple", DarkPurple },
        { "gold", Gold },
        { "gray", Gray },
        { "dark_gray", DarkGray },
        { "blue", Blue },
        { "green", Green },
        { "aqua", Aqua },
        { "red", Red },
        { "light_purple", LightPurple },
        { "yellow", Yellow },
        { "white", White }
    }.ToFrozenDictionary();
}
