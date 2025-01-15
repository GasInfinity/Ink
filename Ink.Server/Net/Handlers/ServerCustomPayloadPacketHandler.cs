using System.Buffers;
using Ink.Net;
using Ink.Net.Packets.Common;
using Ink.Registries;
using Ink.Util;

namespace Ink.Server.Net.Handlers;

public abstract class ServerCustomPayloadPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundCustomPayload>
{
    public sealed override void Handle(in ServerboundCustomPayload packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        switch(packet.Channel)
        {
            case var brand when brand is { Namespace: "minecraft", Path: "brand"}:
                {
                    if(JUtf8String.TryDecode(packet.Data, out _, out ctx.ClientBrand) != OperationStatus.Done)
                        break;

                    HandleBrand(ctx.ClientBrand, connection, ctx);
                    break;
                }
            default: 
                {
                    Handle(packet.Channel, packet.Data, connection, ctx);
                    break;
                }
        }
    }

    protected abstract void Handle(Identifier channel, byte[] data, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx);

    protected virtual void HandleBrand(string brand, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
    }
}
