using Rena.Native.Extensions;

namespace Ink.Util;

public readonly struct BitSet
{
    public const int BackingArrayByteSize = sizeof(long);
    public const int BackingArrayBitSize = sizeof(long) * 8;

    private readonly long[] backing;
    private readonly int bitLength;

    public readonly ReadOnlySpan<long> BackingData
        => this.backing;

    public readonly int BackingLength
        => this.backing.Length;

    public readonly int BitLength
        => this.bitLength;

    public bool this[int bitIndex]
    {
        get
        {
            int byteIndex = bitIndex / BackingArrayBitSize;
            int shiftIndex = bitIndex % BackingArrayBitSize;

            return (((this.backing[byteIndex]) >> shiftIndex) & 1) != 0;
        }
        set
        {
            int byteIndex = bitIndex / BackingArrayBitSize;
            int shiftIndex = bitIndex % BackingArrayBitSize;
            int mask = (1 << shiftIndex);

            ref long data = ref backing[byteIndex];
            data = (byte)((data & mask) | ((long)value.AsByte() << shiftIndex));
        }
    }


    public BitSet(int bitLength)
    {
        this.bitLength = bitLength;
        this.backing = new long[(bitLength + BackingArrayBitSize - 1) / BackingArrayBitSize];
    }
}
