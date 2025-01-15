using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ink.Util;

/// <summary>
/// Provides methods to encode and decode generic VarInts
/// Fast paths adapted from https://github.com/as-com/varint-simd </summary>
/// <typeparam name="TBacking">The backing integer</typeparam>
public static class VarInteger<TBacking>
    where TBacking : unmanaged, IBinaryInteger<TBacking>, IUnsignedNumber<TBacking>
{
    const byte HighBit = 0b1000_0000;
    const byte HighMask = 0b0111_1111;

    public static int MaxByteCount
        => ((Unsafe.SizeOf<TBacking>() * 8) + 6) / 7;

    public static int BytesNeeded
        => MaxByteCount < 8 ? 8 : MaxByteCount;

    public static byte LastByte
        => byte.CreateTruncating(TBacking.AllBitsSet >> (int.CreateSaturating(Unsafe.SizeOf<TBacking>()) * 7));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryEncode(Span<byte> span, out int bytesWritten, TBacking value)
    {
        Unsafe.SkipInit(out bytesWritten);

        bool bigEnough = MaxByteCount < 8 && span.Length >= BytesNeeded;
        Span<byte> data = bigEnough ? span : stackalloc byte[BytesNeeded];

        if(!bigEnough)
            span.CopyTo(data);

        bytesWritten = EncodeUnsafe(ref MemoryMarshal.GetReference(data), value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int EncodeUnsafe(ref byte data, TBacking value)
    {
        if (MaxByteCount < 8)
            return EncodeSmallUnsafe(ref data, value);

        return EncodeUniversalUnsafe(ref data, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationStatus TryDecode(ReadOnlySpan<byte> span, out int bytesRead, out TBacking result)
    {
        result = TBacking.Zero;
        bytesRead = 0;
        int shift = 0;

        while (true)
        {
            if (bytesRead > MaxByteCount)
                return OperationStatus.InvalidData;

            if (bytesRead >= span.Length)
                return OperationStatus.NeedMoreData;

            byte b = span[bytesRead++];

            if ((b & HighBit) == 0)
            {
                result |= TBacking.CreateTruncating(b) << shift;
                return OperationStatus.Done;
            }
            else
            {
                result |= TBacking.CreateTruncating(b & HighMask) << shift;
                shift += 7;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetByteCount(TBacking value)
        => ((Unsafe.SizeOf<TBacking>() * 8) - int.CreateTruncating(TBacking.LeadingZeroCount(value | TBacking.One)) + 6) / 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int EncodeUniversalUnsafe(ref byte data, TBacking value)
    {
        int bytesWritten = 0;
        while (true)
        {
            ++bytesWritten;

            byte valueByte = byte.CreateTruncating(value);
            if (value < TBacking.CreateTruncating(HighBit))
            {
                data = valueByte;
                return bytesWritten;
            }
            else
            {
                data = (byte)(valueByte | HighBit);
                data = ref Unsafe.Add(ref data, 1);
                value >>>= 7;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int EncodeSmallUnsafe(ref byte data, TBacking value)
    {
        const ulong Msb = 0x8080808080808080;

        ulong splitted = SplitSmall(value);
        int bytesWritten = GetByteCountSplittedSmall(splitted);
        // ulong splittedMostSignificant = 0xFFFFFFFFFFFFFFFF >>> ((8 - bytesWritten + 1) * 8 - 1);
        // ulong final = splitted | (splittedMostSignificant & Msb);
        // The JIT can't do this kind of optimization
        int shift = (71 - (bytesWritten << 3));
        ulong final = splitted | (Msb << shift) >>> shift;

        Unsafe.As<byte, ulong>(ref data) = final;
        return bytesWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetByteCountSplittedSmall(ulong value)
        => 8 - (((int)ulong.LeadingZeroCount(value) - 1) >>> 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitSmall(TBacking value)
    {
        if (MaxByteCount > 8)
            ThrowNotSupported();

        ulong valueLong = ulong.CreateTruncating(value);

#if USE_BMI2_PDEP
        if (Bmi2.X64.IsSupported)
            return SplitPDepSmall(value);
#endif
        return SplitBitwiseSmall(value);
    }

#if USE_BMI2_PDEP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitPDepSmall(TBacking value)
    {
        const ulong pdepMask = 0x7f7f7f7f7f7f7f7f;

        if (!Bmi2.X64.IsSupported)
            ThrowNotSupported();

        return Bmi2.X64.ParallelBitDeposit(ulong.CreateSaturating(value), pdepMask);
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SplitBitwiseSmall(TBacking value) // We need to split this into separate methods because the JIT does not like big methods and WONT INLINE THIS!
    {
        static ulong SplitOneByte(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x80) << 1);

        static ulong SplitTwoBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0xC000) << 2);

        static ulong SplitThreeBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0x1F_C000) << 2) | ((valueLong & 0xE00_000) << 3);

        static ulong SplitFourBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0x1F_C000) << 2) | ((valueLong & 0xFE0_0000) << 3) | ((valueLong & 0xF000_0000) << 4);

        static ulong SplitFiveBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0x1F_C000) << 2) | ((valueLong & 0xFE0_0000) << 3) | ((valueLong & 0x7_F000_0000) << 4) | ((valueLong & 0xF8_0000_0000) << 5);

        static ulong SplitSixBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0x1F_C000) << 2) | ((valueLong & 0xFE0_0000) << 3) | ((valueLong & 0x7_F000_0000) << 4) | ((valueLong & 0x3F8_0000_0000) << 5) | ((valueLong & 0xFC00_0000_0000) << 6);

        static ulong SplitSevenBytes(ulong valueLong)
            => (valueLong & 0x7F) | ((valueLong & 0x3F80) << 1) | ((valueLong & 0x1F_C000) << 2) | ((valueLong & 0xFE0_0000) << 3) | ((valueLong & 0x7_F000_0000) << 4) | ((valueLong & 0x3F8_0000_0000) << 5) | ((valueLong & 0x1_FC00_0000_0000) << 6) | ((valueLong & 0xFE_0000_0000_0000) << 7);

        ulong valueLong = ulong.CreateTruncating(value);

        // I would like to use a switch but the JIT won't inline everything. It seems it doesn't do good DCE on a switch before inlining.
        if (Unsafe.SizeOf<TBacking>() == 1)
            return SplitOneByte(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 2)
            return SplitTwoBytes(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 3)
            return SplitThreeBytes(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 4)
            return SplitFourBytes(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 5)
            return SplitFiveBytes(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 6)
            return SplitSixBytes(valueLong);

        if (Unsafe.SizeOf<TBacking>() == 7)
            return SplitSevenBytes(valueLong);

        ThrowNotSupported();
        return 0;
    }

    [DoesNotReturn]
    private static void ThrowNotSupported()
        => throw new NotSupportedException();
}
