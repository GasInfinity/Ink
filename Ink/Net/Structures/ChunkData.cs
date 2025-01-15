using System.Buffers;
using Ink.Chunks;

namespace Ink.Net.Structures;

// TODO: This can be done better?
public readonly record struct ChunkData(ReadOnlyMemory<byte> Data)
{
    public void Write(IBufferWriter<byte> writer)
    {
        writer.Write(Data.Span);
    }

    public static bool TryRead(ReadOnlySpan<byte> payload, out int bytesRead, out ChunkData result)
    {
        bytesRead = default;
        result = default;
        return false;
    }
}
