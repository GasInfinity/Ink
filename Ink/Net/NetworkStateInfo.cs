using Ink.Registries;

namespace Ink.Net;

public sealed class NetworkStateInfo(NetworkState State, FrozenRegistry<IPacketInfo> Serverbound, FrozenRegistry<IPacketInfo> Clientbound)
{
    public readonly NetworkState State = State;
    public readonly FrozenRegistry<IPacketInfo> Serverbound = Serverbound;
    public readonly FrozenRegistry<IPacketInfo> Clientbound = Clientbound;

    public FrozenRegistry<IPacketInfo> DirectedRegistry(NetworkDirection direction)
        => direction == NetworkDirection.Clientbound ? Clientbound : Serverbound;
}
