using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Rena.Native.Buffers;
using Ink.Nbt;
using Ink.Nbt.Tags;
using Rena.Native.Buffers.Extensions;
using Ink.Entities;
using Rena.Native.Extensions;
using System.Runtime.InteropServices;
using Ink.Nbt.Serialization;
using System.Text.Unicode;
using Ink.Nbt.Serialization.Metadata;

namespace Ink.Util.Extensions;

public static class MinecraftBufferWriterExtensions
{
    public static void WriteUtf8Bytes(this IBufferWriter<byte> writer, ReadOnlySpan<char> value) // TODO: This is general purpose enough, move to Rena
    {
        Span<byte> encoded = stackalloc byte[512]; // TODO: Make a helper
        while(Utf8.FromUtf16(value, encoded, out int charsRead, out int bytesWritten) == OperationStatus.DestinationTooSmall)
        {
            value = value[charsRead..]; 
            writer.Write(encoded[..bytesWritten]);
        }
    }

    public static void WriteUuid(this IBufferWriter<byte> writer, Uuid value)
    {
        Span<byte> needed = writer.GetSpan(Unsafe.SizeOf<Uuid>());
        BinaryPrimitives.WriteInt64BigEndian(needed, value.MostSignificantBytes);
        BinaryPrimitives.WriteInt64BigEndian(needed.SliceUnsafe(sizeof(long)), value.LeastSignificantBytes);
        writer.Advance(Unsafe.SizeOf<Uuid>());
    }

    public static void WriteVarInteger(this IBufferWriter<byte> writer, int value)
        => WriteVarBinaryInteger(writer, (uint)value);

    public static void WriteVarLong(this IBufferWriter<byte> writer, long value)
        => WriteVarBinaryInteger(writer, (ulong)value);

    public static void WriteVarBinaryInteger<TInteger>(this IBufferWriter<byte> writer, TInteger value)
        where TInteger : unmanaged, IBinaryInteger<TInteger>, IUnsignedNumber<TInteger>
    {
        Span<byte> needed = writer.GetSpan(VarInteger<TInteger>.BytesNeeded);
        writer.Advance(VarInteger<TInteger>.EncodeUnsafe(ref MemoryMarshal.GetReference(needed), value));
    }

    public static void WriteJUtf8String(this IBufferWriter<byte> writer, ReadOnlySpan<char> value)
    {
        Span<byte> needed = writer.GetSpan(JUtf8String.GetNeededByteCount(value));

        if(JUtf8String.TryEncode(needed, out int bytesWritten, value) != OperationStatus.Done)
            Debug.Fail(string.Empty);

        writer.Advance(bytesWritten);
    }

    public static void WriteJUtf8String(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
    {
        Span<byte> needed = writer.GetSpan(JUtf8String.GetNeededByteCount(value));

        if (JUtf8String.TryEncode(needed, out int bytesWritten, value) != OperationStatus.Done)
            Debug.Fail(string.Empty);
        writer.Advance(bytesWritten);
    }

    public static void WriteJsonJUtf8<T>(this IBufferWriter<byte> writer, T value, JsonTypeInfo<T> typeInfo)
    {
        using PooledArrayBufferWriter<byte> bufferWriter = new(ArrayPool<byte>.Shared);

        using (Utf8JsonWriter jsonWriter = new(bufferWriter))
            JsonSerializer.Serialize<T>(jsonWriter, value, typeInfo);

        writer.WriteVarInteger(bufferWriter.WrittenCount);
        writer.Write(bufferWriter.WrittenSpan);
    }

    public static void WriteNbt(this IBufferWriter<byte> writer, RootTag root)
    {
        using NbtWriter<JavaNbtDatatypeWriter> nbtWriter = new(writer, new(ShouldValidate: true, MaxDepth: 128));
        root.WriteTo(nbtWriter);
    }

    public static void WriteNbt<T>(this IBufferWriter<byte> writer, T value, NbtTypeInfo<T> typeInfo)
    {
        using NbtWriter<JavaNbtDatatypeWriter> nbtWriter = new(writer, new(ShouldValidate: true, MaxDepth: 128));
        NbtSerializer.Serialize<JavaNbtDatatypeWriter, T>(nbtWriter, null, value, typeInfo);
    }

    public static void WriteBitSet(this IBufferWriter<byte> writer, BitSet bitSet)
    {
        writer.WriteVarInteger(bitSet.BackingLength);

        foreach (var value in bitSet.BackingData)
            writer.WriteInt64(value, false);
    }

    public static void WriteMetaHeader(this IBufferWriter<byte> writer, byte index, EntityMetaType type)
    {
        writer.WriteRaw(index);
        writer.WriteVarInteger((int)type);
    }

    public static bool TryWriteOptional(this IBufferWriter<byte> writer, bool condition)
    {
        writer.WriteRaw(condition.AsByte());
        return condition;
    }
}
