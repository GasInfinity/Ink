using Ink.Net;
using Ink.Net.Packets.Common;
using Ink.Net.Packets.Configuration;
using Ink.Net.Structures;
using Ink.Registries;
using Ink.Text;
using Ink.Util;
using Rena.Native.Extensions;

namespace Ink.Server.Net.Handlers;

public sealed class ConfigurationServerStateHandler : ServerKeptAlivePacketStateHandler
{
    private enum State
    {
        PreparingKnownPacks,
        WaitingKnownPacks,
        PreparingRegistryData,
        PreparingTagData,
        WaitingAck
    }

    private sealed class Context
    {
        public State CurrentState = State.PreparingKnownPacks;
        public VersionedIdentifier[]? ClientKnownPacks;
    }

    private sealed class SelectKnownPacksPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundSelectKnownPacks>
    {
        public override void Handle(in ServerboundSelectKnownPacks packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();

            if(c.CurrentState != State.WaitingKnownPacks)
            {
                connection.Disconnect(TextPart.String($"Invalid {nameof(ServerboundSelectKnownPacks)} packet."));
                return;
            }

            c.ClientKnownPacks = packet.KnownPacks;
            c.CurrentState = State.PreparingRegistryData;
            
            foreach(var i in c.ClientKnownPacks)
                Console.WriteLine($"Client known pack: {i}");
        }
    }

    private sealed class ClientInformationPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundClientInformation>
    {
        public override void Handle(in ServerboundClientInformation packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();
            ctx.ClientInformation = packet;
        }
    }

    private sealed class ConfigurationCustomPayloadPacketHandler : ServerCustomPayloadPacketHandler
    {
        protected override void Handle(Identifier channel, byte[] data, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
        }
    }

    private sealed class FinishConfigurationPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundFinishConfiguration>
    {
        public override void Handle(in ServerboundFinishConfiguration packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();

            if(c.CurrentState != State.WaitingAck)
            {
                connection.Abort(TextPart.String($"Invalid {nameof(ServerboundFinishConfiguration)} packet.")); // Why abort? The client has switched state!
                return;
            }

            ctx.SwitchState(NetworkState.Play);
        }
    }

    public ConfigurationServerStateHandler()
        : base(NetworkStates.ConfigurationStateInfo)
    {
        Register(new SelectKnownPacksPacketHandler());
        Register(new ClientInformationPacketHandler());
        Register(new ConfigurationCustomPayloadPacketHandler());
        Register(new FinishConfigurationPacketHandler());
        Freeze();
    }

    public override void Setup(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Setup(connection, ctx);

        ctx.StateContext = new Context();
    }

    public override void Tick(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Tick(connection, ctx);

        Context c = ctx.StateContext!.CastUnsafe<Context>();

        switch(c.CurrentState)
        {
            case State.PreparingKnownPacks:
                {
                    connection.Send(new ClientboundSelectKnownPacks(ctx.Connection.GameHandler.RegistryManager.KnownPacks.ToArray()));
                    c.CurrentState = State.WaitingKnownPacks;
                    break;
                }
            case State.WaitingKnownPacks:
                {
                    break;
                }
            case State.PreparingRegistryData:
                {
                    foreach(RegistryKey key in RegistriesInfo.SyncedRegistries)
                    {
                        IReadOnlyRegistry registry = ctx.Connection.GameHandler.RegistryManager.Registries.Get(key.Value)!;

                        if(registry.Count > 0)
                            connection.Send(new ClientboundRegistryData(new SyncedRegistry(key.Value, registry, true))); 
                    }
                    c.CurrentState = State.PreparingTagData;
                    break;
                }
            case State.PreparingTagData:
                {
                    connection.Send(new ClientboundCustomPayload(KnownChannels.Brand, ServerConstants.ServerBrand));
                    connection.Send(new ClientboundFinishConfiguration());
                    c.CurrentState = State.WaitingAck;
                    break;
                }
            case State.WaitingAck:
                {
                    break;
                }
        }
    }

    public override void Disconnected(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Disconnected(connection, ctx, reason);
        connection.Send(new ClientboundDisconnect(reason));
    }

    public override void Terminated(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Terminated(connection, ctx, reason);
        ctx.Logger.Disconnected(connection.Id, reason.ToPlainText(IContentDataProvider.Null));
    }
}


