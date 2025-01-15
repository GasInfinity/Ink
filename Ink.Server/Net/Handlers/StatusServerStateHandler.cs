using Ink.Net;
using Ink.Net.Packets.Status;
using Ink.Net.Packets.Common;

namespace Ink.Server.Net.Handlers;

public sealed class StatusServerStateHandler : ServerPacketStateHandler
{
    private sealed class StatusRequestPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundStatusRequest>
    {
        public override void Handle(in ServerboundStatusRequest packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            connection.Send(new ClientboundStatusResponse(Ink.Util.ServerStatus.Default));
        }
    }

    private sealed class PingRequestPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundPingRequest>
    {
        public override void Handle(in ServerboundPingRequest packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            connection.Send(new ClientboundPongResponse(packet.Payload));
            connection.Disconnect();
        }
    }

    public StatusServerStateHandler()
        : base(NetworkStates.StatusStateInfo)
    {
        Register(new StatusRequestPacketHandler());
        Register(new PingRequestPacketHandler());
        Freeze();
    }
}
