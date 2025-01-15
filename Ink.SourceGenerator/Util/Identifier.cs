using System.Diagnostics.CodeAnalysis;

namespace Ink.SourceGenerator.Util;

// HACK: Duplicated from Ink.Registries
public readonly record struct Identifier(string Namespace, string Path) : ISpanFormattable, ISpanParsable<Identifier>
{
    public const string DefaultNamespace = "minecraft";
    public readonly string Namespace = Namespace;
    public readonly string Path = Path;

    public override string ToString()
        => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"{Namespace}:{Path}";

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => destination.TryWrite($"{Namespace}:{Path}", out charsWritten);

    public static Identifier Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if(TryParse(s, provider, out var result))
            return result;

        throw new ArgumentException($"Invalid {nameof(Identifier)}");
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Identifier result)
    {
        int sepIdx = s.IndexOf(':');

        if(sepIdx == -1)
        {
            result = new(DefaultNamespace, s.ToString());
            return true;
        }

        result = new(s[..sepIdx].ToString(), s[(sepIdx + 1)..].ToString());
        return true;
    }

    public static Identifier Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Identifier result)
        => TryParse(s.AsSpan(), provider, out result);

    public static Identifier Vanilla(string path)
        => new(DefaultNamespace, path);
}
