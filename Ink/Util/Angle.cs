using System.Buffers;
using Rena.Native.Buffers.Extensions;

namespace Ink.Util;

public readonly struct Angle(byte Value)
{
    const float ByteSingleConversion = 256f / 360f;

    public readonly byte Value = Value;

    public float NormalizedValue
        => FromAngle(Value);

    public Angle(float value) : this(ToAngle(value))
    { }

    public void Write(IBufferWriter<byte> writer)
        => writer.WriteRaw(Value);

    public static byte ToAngle(float value)
        => (byte)(ClampRotation(value) * ByteSingleConversion);

    public static float FromAngle(byte value)
        => value / ByteSingleConversion;

    public static explicit operator byte(Angle value)
        => value.Value;

    public static explicit operator Angle(byte value)
        => new(value);

    public static float ClampRotation(float rotation)
        => (rotation % 360) is float clamped && clamped < 0 ? clamped + 360f : clamped;
}
