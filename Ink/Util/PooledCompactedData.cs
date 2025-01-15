using Rena.Native.Buffers.Extensions;
using Rena.Native.Extensions;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ink.Util.Extensions;

namespace Ink.Util;

/// <summary>
/// Stores integers with the given bitsPerEntry, the data won't span over multiple longs. It uses ArrayPool<byte> for better efficiency (FIXME: Could this cause memory alignment issues?)
/// Should we use arrays directly without pooling? If we pool the arrays we may have some unused bytes ~100...~800 but it may help by not having GC pressure.
/// </summary>
public readonly struct PooledCompactedData : IDisposable
{
    public const byte BackingDataByteSize = sizeof(ulong);
    public const byte BackingDataBitSize = BackingDataByteSize * 8;
    public const int MaxCapacitySupported = 4096;

    private readonly byte[] rawData;
    private readonly int entryMask;
    private readonly byte elementsPerBacking;

    public readonly int DataCapacity;
    public readonly byte BitsPerEntry;

    public Span<long> BackingData
        => MemoryMarshal.CreateSpan(ref Unsafe.As<byte, long>(ref MemoryMarshal.GetArrayDataReference(rawData)), (int)((uint)rawData.Length / BackingDataByteSize));

    public int this[int index]
    {
        get
        {
            ref long byteIndex = ref GetLocationUnsafely(index, out int bitIndex);
            return GetUnsafely(ref byteIndex, bitIndex);
        }
        set
        {
            ref long byteIndex = ref GetLocationUnsafely(index, out int bitIndex);
            SetUnsafely(ref byteIndex, bitIndex, value);
        }
    }

    public PooledCompactedData(int capacity, byte bitsPerEntry)
    {
        Debug.Assert(capacity <= MaxCapacitySupported);
        Debug.Assert(bitsPerEntry <= BackingDataBitSize);

        BitsPerEntry = bitsPerEntry;

        elementsPerBacking = (byte)(BackingDataBitSize / (uint)bitsPerEntry);
        entryMask = (int)((uint)1 << bitsPerEntry) - 1;

        DataCapacity = OptimizedBackingCapacity(capacity, elementsPerBacking);
        rawData = ArrayPool<byte>.Shared.Rent(DataCapacity * BackingDataByteSize);

        rawData.AsSpan().Clear();
    }

    public PooledCompactedData Grow(int capacity, byte newBitsPerEntry)
    {
        PooledCompactedData newSectionData = new(capacity, newBitsPerEntry);

        if (rawData == null)
            return newSectionData;

        for (int i = 0; i < capacity; ++i)
            newSectionData[i] = this[i];

        return newSectionData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref long GetLocationUnsafely(int index, out int bitIndex)
    {
        int longIndex = index / elementsPerBacking;
        bitIndex = (index - (longIndex * elementsPerBacking)) * BitsPerEntry;

        Debug.Assert(rawData != null, "The backing array must not be null");
        Debug.Assert(longIndex < rawData.Length, "Index should be non negative and less than the capacity");
        return ref Unsafe.As<byte, long>(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetArrayDataReference(rawData), longIndex * BackingDataByteSize));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUnsafely(ref long byteIndex, int bitIndex, int value)
    {
        Debug.Assert(entryMask + 1 > value, "Value must fit!");
        byteIndex = (byteIndex & ~((long)entryMask << bitIndex)) | (long)value << bitIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetUnsafely(ref long byteIndex, int bitIndex)
        => (int)(byteIndex >>> bitIndex) & entryMask;

    public void Write(IBufferWriter<byte> writer)
    {
        if (rawData == null) // Is true when a palette is SingleValued
        {
            writer.WriteVarInteger(0);
            return;
        }

        writer.WriteVarInteger(DataCapacity);

        Span<long> rawBacking = BackingData;
        for (int i = 0; i < DataCapacity; ++i)
            writer.WriteInt64(rawBacking.GetUnsafe(i), false);
    }

    public void Dispose()
    {
        if (rawData != null)
            ArrayPool<byte>.Shared.Return(rawData);
    }

    public static int OptimizedBackingCapacity(int capacity, int elementsPerBacking)
        => (int)((uint)(capacity + elementsPerBacking - 1) / (uint)elementsPerBacking);
}
