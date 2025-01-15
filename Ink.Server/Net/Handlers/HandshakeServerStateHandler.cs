using Ink.Net;
using Ink.Net.Packets.Handshake;
using Ink.Text;

namespace Ink.Server.Net.Handlers;

public sealed class HandshakeServerStateHandler : ServerPacketStateHandler
{
    private sealed class HandshakePacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundIntention>
    {
        public override void Handle(in ServerboundIntention packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ctx.ProtocolVersion = packet.ProtocolVersion;

            switch(packet.NextState)
            {
                case NetworkState.Transfer:
                    {
                        connection.Disconnect(TextPart.String("TODO: Transfer"));
                        break;
                    }
                case NetworkState.Status:
                case NetworkState.Login:
                    {
                        ctx.SwitchState(packet.NextState);
                        break;
                    }
                default:
                    {
                        connection.Disconnect(TextPart.String($"Invalid {nameof(ServerboundIntention)} packet."));
                        break;
                    }
            }

        }
    }

    public HandshakeServerStateHandler()
        : base(NetworkStates.HandshakeStateInfo)
    {
        Register(new HandshakePacketHandler());
        Freeze();
    }
}
