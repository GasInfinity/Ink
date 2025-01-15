using Ink.Registries;

namespace Ink.Net;

public interface IPacketInfo : IHasLocation
{
    NetworkDirection Direction { get; }
    new Identifier Location { get; }
    Type Packet { get; }

    Identifier IHasLocation.Location => Location;
}

public sealed class PacketInfo<TPacket> : IPacketInfo
    where TPacket : struct, IPacket<TPacket>
{
    public NetworkDirection Direction => TPacket.PacketDirection;
    public Identifier Location => TPacket.PacketLocation;
    public Type Packet => typeof(TPacket);
}
