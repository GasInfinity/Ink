using System.Buffers;
using System.Buffers.Binary;
using Ink.Chunks;
using Rena.Mathematics;
using Rena.Native.Buffers.Extensions;

namespace Ink.Math;

public readonly record struct BlockPosition(Vec3<int> Vec)
{
    public readonly Vec3<int> Vec = Vec;

    public readonly int X
        => Vec.X;
    public readonly int Y
        => Vec.Y;
    public readonly int Z
        => Vec.Z;

    public BlockPosition(Vec3<double> position) : this(new Vec3<int>((int)double.Floor(position.X), (int)double.Floor(position.Y), (int)double.Floor(position.Z)))
    {
    }

    public BlockPosition(int x, int y, int z) : this(new Vec3<int>(x, y, z))
    {
    }

    public BlockPosition Relative(BlockFace face)
        => Relative(FromBlockFace(face));

    public BlockPosition Relative(int x = 0, int y = 0, int z = 0)
        => Relative(new Vec3<int>(x, y, z));

    public BlockPosition Relative(BlockPosition pos)
        => Relative(pos.Vec);
    
    public BlockPosition Relative(Vec3<int> pos)
        => new(Vec + pos);
    
    public SectionPosition ToSectionPosition()
        => new(X >> Section.ShiftAmount, Y >> Section.ShiftAmount, Z >> Section.ShiftAmount);

    public ChunkPosition ToChunkPosition()
        => new(X >> Chunk.ShiftAmount, Z >> Chunk.ShiftAmount);

    public void Write(IBufferWriter<byte> writer)
        => writer.WriteInt64(Encode(), false);

    public long Encode()
        => ((long)X & 0x3FFFFFF) << 38 | ((long)Z & 0x3FFFFFF) << 12 | (long)Y & 0xFFF;
    
    public static BlockPosition Decode(long encoded)
        => new((int)(encoded >> 38), (int)(encoded << 52 >> 52), (int)(encoded << 26 >> 38));
    
    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out BlockPosition value)
    {
        if(data.Length < sizeof(long))
        {
            value = default;
            bytesRead = 0;
            return false;
        }

        value = BlockPosition.Decode(BinaryPrimitives.ReadInt64BigEndian(data));
        bytesRead = sizeof(long);
        return true;
    }

    static ReadOnlySpan<sbyte> RelativeBlockFaces => [0, -1, 0, 
                                                      0, 1, 0, 
                                                      0, 0, -1, 
                                                      0, 0, 1,
                                                      -1, 0, 0,
                                                      1, 0, 0];
    public static BlockPosition FromBlockFace(BlockFace face)
    {
        int index = ((int)face * 3);
        return new(RelativeBlockFaces[index], RelativeBlockFaces[index + 1], RelativeBlockFaces[index + 2]);
    }

    public static explicit operator BlockPosition(in Vec3<double> position)
        => new(position);

    public static implicit operator Vec3<double>(BlockPosition position)
        => Vec3<double>.CreateTruncating(position.Vec);
}
