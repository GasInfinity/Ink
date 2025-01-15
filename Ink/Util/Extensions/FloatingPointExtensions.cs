namespace Ink.Util.Extensions;

public static class FloatingPointExtensions
{
    public const double Tolerance = 1e-7;
    public const float SingleTolerance = 1e-5f;

    public static bool AlmostEqual(this double x, double y)
        => double.Abs(x - y) < Tolerance;

    public static bool AlmostEqual(this float x, float y)
        => float.Abs(x - y) < SingleTolerance;
}
