using Rena.Native.Extensions;
using System.Buffers;
using System.Text;
using System.Text.Unicode;

namespace Ink.Util;

public static class JUtf8String
{
    public static OperationStatus TryDecode(ReadOnlySpan<byte> data, out int bytesRead, out string result)
    {
        result = string.Empty;
        OperationStatus status = VarInteger<uint>.TryDecode(data, out bytesRead, out uint unsignedLength);
        int length = (int)unsignedLength;

        if (status != OperationStatus.Done)
            return status;
        data = data.SliceUnsafe(bytesRead);

        if (data.Length < length)
            return OperationStatus.NeedMoreData;

        bytesRead += length;
        result = Encoding.UTF8.GetString(data[..length]);
        return OperationStatus.Done;
    }

    public static OperationStatus TryEncode(Span<byte> data, out int bytesWritten, ReadOnlySpan<char> value)
    {
        if (!VarInteger<uint>.TryEncode(data, out bytesWritten, (uint)GetByteCount(value)))
            return OperationStatus.DestinationTooSmall;

        data = data.SliceUnsafe(bytesWritten);
        OperationStatus status = Utf8.FromUtf16(value, data, out _, out int stringBytesWritten);

        if (status != OperationStatus.Done)
            return status;

        bytesWritten += stringBytesWritten;
        return OperationStatus.Done;
    }

    public static OperationStatus TryEncode(Span<byte> data, out int bytesWritten, ReadOnlySpan<byte> value)
    {
        if (!VarInteger<uint>.TryEncode(data, out bytesWritten, (uint)value.Length))
            return OperationStatus.DestinationTooSmall;

        data = data.SliceUnsafe(bytesWritten);

        if (data.Length < value.Length)
            return OperationStatus.DestinationTooSmall;
        value.CopyTo(data);

        bytesWritten += bytesWritten + value.Length;
        return OperationStatus.Done;
    }

    public static int GetNeededByteCount(ReadOnlySpan<byte> value)
        => VarInteger<uint>.BytesNeeded + value.Length;

    public static int GetNeededByteCount(ReadOnlySpan<char> value)
        => VarInteger<uint>.BytesNeeded + GetMaxByteCount(value);

    public static int GetTotalByteCount(ReadOnlySpan<char> value)
        => VarInteger<uint>.GetByteCount((uint)value.Length) + GetByteCount(value);

    public static int GetTotalByteCount(ReadOnlySpan<byte> value)
        => VarInteger<uint>.GetByteCount((uint)value.Length) + value.Length;

    public static int GetMaxByteCount(ReadOnlySpan<char> value)
        => Encoding.UTF8.GetMaxByteCount(value.Length);

    public static int GetByteCount(ReadOnlySpan<char> value)
        => Encoding.UTF8.GetByteCount(value);
}
