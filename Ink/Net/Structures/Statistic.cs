using System.Buffers;

namespace Ink.Net.Structures;

// FIXME: This
public readonly record struct Statistic()
{
    public void Write(IBufferWriter<byte> writer)
    {
        // TODO: This
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out Statistic value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}
