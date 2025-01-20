using System.Runtime.CompilerServices;

namespace Ink.Util;

public static class Utilities
{
    public static int Modulo(int value, int mod)
        => (value % mod + mod) % mod;

    public static byte BitSize(int value)
        => (byte)(uint.Log2((uint)(value - 1) | 1) + 1);

    public static byte BitsNeeded(int value)
        => (byte)(uint.Log2((uint)value | 1) + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetInteger<T>(T value, out int result) // TODO: Simplify this
        where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 1)
        {
            result = Unsafe.BitCast<T, byte>(value);
            return true;
        }

        if (Unsafe.SizeOf<T>() == 2)
        {
            result = Unsafe.BitCast<T, ushort>(value);
            return true;
        }

        if (Unsafe.SizeOf<T>() == 4)
        {
            result = Unsafe.BitCast<T, int>(value);
            return true;
        }

        result = default;
        return false;
    }
}
