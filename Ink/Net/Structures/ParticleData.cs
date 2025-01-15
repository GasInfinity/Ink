using System.Buffers;

namespace Ink.Net.Structures;

// FIXME: This
public readonly record struct ParticleData()
{
    public void Write(IBufferWriter<byte> writer)
    {
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out ParticleData value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}
