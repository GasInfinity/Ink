using System.Buffers;
using Ink.Chunks;
using Rena.Mathematics;
using Rena.Native.Buffers.Extensions;

namespace Ink.Math;

public readonly record struct SectionPosition(Vec3<int> Value)
{
    public readonly Vec3<int> Vec = Value;

    public readonly int X
        => Vec.X;
    public readonly int Y
        => Vec.Y;
    public readonly int Z
        => Vec.Z;

    public SectionPosition(int x, int y, int z) : this(new(x, y, z))
    {
    }

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteInt64(Encode(), false);
    }

    public static bool TryRead(ReadOnlySpan<byte> payload, out int bytesRead, out SectionPosition result)
    {
        bytesRead = default;
        result = default;
        return false;
    }

    public ChunkPosition ToChunkPosition()
        => new(Vec.Xz);

    public BlockPosition ToAbsolute(int x, int y, int z)
        => ToAbsolute(new(x, y, z));

    public BlockPosition ToAbsolute(Vec3<int> relative)
        => new(new Vec3<int>(X << Section.ShiftAmount, Y << Section.ShiftAmount, Z << Section.ShiftAmount) + relative);

    public long Encode()
        => (((long)X) << 42) | (((long)Z) << 20) | ((long)Y);
}
