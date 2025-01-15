namespace Ink.Util;

public static class Int16Delta
{
    public const int MaxDelta = 8;

    public static short ToDelta(double newValue, double lastValue)
        => (short) (((newValue * 32) - (lastValue * 32)) * 128);

    public static short ToDelta(double delta)
        => (short)((delta * (32.0 * 128.0)));

    public static double FromDelta(short delta)
        => (delta / (128.0 * 32.0));
}
