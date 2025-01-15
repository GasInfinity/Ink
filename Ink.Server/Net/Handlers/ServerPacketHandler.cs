using Ink.Net;

namespace Ink.Server.Net.Handlers;

public abstract class ServerPacketStateHandler : PacketStateHandler<ServerNetworkConnection.ServerConnectionContext>
{
    protected ServerPacketStateHandler(NetworkStateInfo stateInfo)
        : base(stateInfo, NetworkDirection.Serverbound)
    {
    }
}
