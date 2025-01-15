using System.Buffers;
using Ink.Registries;

namespace Ink.Net;

public interface IPacket<TPacket> : IHasLocation
    where TPacket : struct, IPacket<TPacket>
{
    static abstract NetworkDirection PacketDirection { get; }
    static abstract Identifier PacketLocation { get; }

    Identifier IHasLocation.Location => TPacket.PacketLocation;

    void Write(IBufferWriter<byte> writer);
    static abstract bool TryRead(ReadOnlySpan<byte> data, out TPacket result);
}
