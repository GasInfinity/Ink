using Ink.Net;
using Ink.Net.Packets.Common;
using Ink.Text;

namespace Ink.Server.Net.Handlers;

public abstract class ServerKeptAlivePacketStateHandler : ServerPacketStateHandler
{
    private sealed class KeepAlivePacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundKeepAlive>
    {
        public override void Handle(in ServerboundKeepAlive packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
            => ctx.TicksSinceKeepAliveResponse = 0;
    }

    protected ServerKeptAlivePacketStateHandler(NetworkStateInfo stateInfo)
        : base(stateInfo)
    {
        Register(new KeepAlivePacketHandler());
    }

    public override void Tick(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Tick(connection, ctx);

        if(ctx.TicksSinceKeepAliveResponse++ >= ServerConstants.TicksKeepAliveTimeout)
        {
            connection.Disconnect(TextPart.String("Timed out"));
            return;
        }
        
        if(ctx.TicksSinceKeepAlive++ >= ServerConstants.TicksBetweenKeepAlives)
        {
            connection.Send(new ClientboundKeepAlive(-1));
            ctx.TicksSinceKeepAlive = 0;
        }
    }
}
