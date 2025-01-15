using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Rena.Mathematics;

namespace Ink.Math;

public readonly record struct AabbDefinition(Vec3<double> Half)
{
    public static AabbDefinition Empty
        => default;

    public readonly Vec3<double> Half = Half;

    public readonly double HalfWidth
        => Half.X;
    public readonly double HalfHeight
        => Half.Y;
    public readonly double HalfDepth
        => Half.Z;

    public double Width
        => HalfWidth * 2;

    public double Height
        => HalfHeight * 2;

    public double Depth
        => HalfDepth * 2;

    public AabbDefinition(double halfWidth, double halfHeight, double halfDepth) : this(new(halfWidth, halfHeight, halfDepth))
    {
    }

    public (double Min, double Max) MinMaxX(double x)
        => MinMax(x, Width);

    public (double Min, double Max) MinMaxY(double y)
        => MinMax(y, Height);

    public (double Min, double Max) MinMaxZ(double z)
        => MinMax(z, Depth);

    public bool Intersects(double centerX, double centerY, double centerZ, double otherCenterX, double otherCenterY, double otherCenterZ, AabbDefinition other)
    {
        double dX = double.Abs(centerX - otherCenterX);
        double dY = double.Abs(centerY - otherCenterY);
        double dZ = double.Abs(centerZ - otherCenterZ);

        double rX = dX - (HalfWidth + other.HalfWidth);
        double rY = dY - (HalfHeight + other.HalfHeight);
        double rZ = dZ - (HalfDepth + other.HalfDepth);

        return rX < 0 & rY < 0 & rZ < 0;
    }

    private static Vector128<double> DepthMul => Vector128.Create(-1.0, 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double Min, double Max) MinMax(double coordinate, double halfDepth)
    {
        if(Vector128.IsHardwareAccelerated)
        {
            Vector128<double> depths = Vector128.Create(halfDepth);
            Vector128<double> negDepths = depths * DepthMul;
            Vector128<double> coordinates = Vector128.Create(coordinate);
            Vector128<double> result = coordinates + negDepths;
            return Unsafe.BitCast<Vector128<double>, (double, double)>(result);
        }

        return (coordinate - halfDepth, coordinate + halfDepth);
    }

    public static AabbDefinition FromSizes(double width, double height, double depth)
        => new(width / 2, height / 2, depth / 2);

    public static AabbDefinition FromSizes(double horizontal, double height)
    {
        double halfHorizontal = horizontal / 2;
        return new(halfHorizontal, height / 2, halfHorizontal);
    }
}
