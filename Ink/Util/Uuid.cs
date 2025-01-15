using Rena.Native.Extensions;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Ink.Util;

public readonly record struct Uuid : ISpanFormattable, IUtf8SpanFormattable, ISpanParsable<Uuid>
{
    public readonly long MostSignificantBytes;
    public readonly long LeastSignificantBytes;

    public Uuid(ReadOnlySpan<byte> data)
    {
        if (data.Length != Unsafe.SizeOf<Uuid>())
            throw new ArgumentException($"Size of data must equal sizeof({nameof(Uuid)}) (16)");

        this = MemoryMarshal.Read<Uuid>(data);
    }

    public Uuid(long mostSignificastBytes, long leastSignificant)
    {
        MostSignificantBytes = mostSignificastBytes;
        LeastSignificantBytes = leastSignificant;
    }

    public Uuid(long mostSignificantBytes, byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
    {
        MostSignificantBytes = mostSignificantBytes;
        LeastSignificantBytes = ((long)a << 56) | ((long)b << 48) | ((long)c << 40) | ((long)d << 32) | ((long)e << 24) | ((long)f << 16) | ((long)g << 8) | h;
    }

    public Uuid(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h, long leastSignificantBytes)
    {
        MostSignificantBytes = ((long)a << 56) | ((long)b << 48) | ((long)c << 40) | ((long)d << 32) | ((long)e << 24) | ((long)f << 16) | ((long)g << 8) | h;
        LeastSignificantBytes = leastSignificantBytes;
    }

    public Uuid(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h,
                byte i, byte j, byte k, byte l, byte m, byte n, byte o, byte p)
    {
        MostSignificantBytes = ((long)a << 56) | ((long)b << 48) | ((long)c << 40) | ((long)d << 32) | ((long)e << 24) | ((long)f << 16) | ((long)g << 8) | h;
        LeastSignificantBytes = ((long)i << 56) | ((long)j << 48) | ((long)k << 40) | ((long)l << 32) | ((long)m << 24) | ((long)n << 16) | ((long)o << 8) | p;
    }

    public Uuid(Guid guid)
    {
        Span<byte> uuid = MemoryMarshal.CreateSpan(ref Unsafe.As<Uuid, byte>(ref this), Unsafe.SizeOf<Uuid>());
        _ = guid.TryWriteBytes(uuid, true, out _);
        uuid[0..8].Reverse();
        uuid[8..16].Reverse();
    }

    public void Write(IBufferWriter<byte> writer)
    {
        Span<byte> needed = writer.GetSpan(Unsafe.SizeOf<Uuid>());
        BinaryPrimitives.WriteInt64BigEndian(needed, MostSignificantBytes);
        BinaryPrimitives.WriteInt64BigEndian(needed[sizeof(long)..], LeastSignificantBytes);
        writer.Advance(Unsafe.SizeOf<Uuid>());
    }

    public Guid AsGuid()
    {
        ReadOnlySpan<byte> uuidBytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Uuid, byte>(ref Unsafe.AsRef(in this)), Unsafe.SizeOf<Uuid>());
        Span<byte> guidBytes = stackalloc byte[Unsafe.SizeOf<Guid>()];
        _ = uuidBytes.TryCopyTo(guidBytes);
        guidBytes[0..8].Reverse();
        guidBytes[8..16].Reverse();
        return new(guidBytes, true);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => AsGuid().TryFormat(destination, out charsWritten, format);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => AsGuid().TryFormat(utf8Destination, out bytesWritten, format);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => AsGuid().ToString(format, formatProvider);

    public override string ToString()
        => AsGuid().ToString();

    public static Uuid Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => new (Guid.Parse(s, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Uuid result)
    {
        if(!Guid.TryParse(s, provider, out Guid guid))
        {
            result = default;
            return false;
        }

        result = new (guid);
        return true;
    }

    public static Uuid Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Uuid result)
        => TryParse(string.IsNullOrEmpty(s) ? default : s.AsSpan(), provider, out result);

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out Uuid result)
    {
        if(data.Length < Unsafe.SizeOf<Uuid>())
        {
            result = default;
            bytesRead = default;
            return false;
        }

        result = new(BinaryPrimitives.ReadInt64BigEndian(data), BinaryPrimitives.ReadInt64BigEndian(data[sizeof(long)..]));
        bytesRead = Unsafe.SizeOf<Uuid>();
        return true;
    }

    public static Uuid FromStringV3(ReadOnlySpan<char> value) // TODO: Optimize this
    {
        int byteCount = Encoding.UTF8.GetByteCount(value);
        byte[] rentedUtf8Data = ArrayPool<byte>.Shared.Rent(byteCount);
        Span<byte> utf8Data = rentedUtf8Data.AsSpan().SliceUnsafe(0, byteCount);
        Encoding.UTF8.GetBytes(value, utf8Data);
        Uuid result = FromBytesV3(utf8Data);
        ArrayPool<byte>.Shared.Return(rentedUtf8Data);
        return result;
    }

    public static Uuid FromBytesV3(ReadOnlySpan<byte> value) // TODO: Optimize this
    {
        Span<byte> digest = stackalloc byte[MD5.HashSizeInBytes];

        if(!MD5.TryHashData(value, digest, out int _))
            Debug.Fail(string.Empty);

        digest[6] = (byte)((digest[6] & 0x0f) | 0x30);
        digest[8] = (byte)((digest[8] & 0x3f) | 0x80);
        return new(digest);
    }

    public static explicit operator Uuid(Guid guid)
        => new(guid);

    public static explicit operator Guid(Uuid uuid)
        => uuid.AsGuid();
}
