using System.Buffers;
using Rena.Mathematics;
using Rena.Native.Buffers.Extensions;

namespace Ink.Util;

public readonly struct Int16Velocity(short Value)
{
    const double Equivalence = 8000;

    public readonly short Value = Value;

    public double NormalizedValue
        => Value / Equivalence;

    public Int16Velocity(double value) : this((short)(value * Equivalence))
    { }

    public void Write(IBufferWriter<byte> writer)
        => writer.WriteInt16(Value, false);

    public static Vec3<short> ToVelocity(in Vec3<double> value)
        => Vec3<short>.CreateTruncating(value * Equivalence);

    public static Vec3<double> FromVelocity(Vec3<short> velocity)
        => Vec3<double>.CreateTruncating(velocity) / Equivalence;
}
