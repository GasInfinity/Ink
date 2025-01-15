namespace Ink.SourceGenerator.Util;

// HACK: Duplicated
public static class Utilities
{
    public static byte BitSize(int value)
        => (byte)(uint.Log2((uint)(value - 1) | 1) + 1);
}
