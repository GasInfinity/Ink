using Ink.Text;
using Ink.Server.Entities;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Ink.Net;
using Ink.Net.Packets.Common;
using System.Collections.Frozen;
using Ink.Server.Net.Handlers;
using Ink.Net.Encryption;
using Ink.Auth;

namespace Ink.Server.Net;

public sealed class ServerNetworkConnection : NetworkConnection<ServerNetworkConnection.ServerConnectionContext>
{
    public class ServerConnectionContext(ServerNetworkConnection connection)
    {
        public readonly ServerNetworkConnection Connection = connection;
        public RSAKeyring ServerKeyring => Connection.serverKeyring;
        public ILogger Logger => Connection.logger;
        public ServerNetworkGameManager GameHandler => Connection.gameHandler;
        public ServerPlayerEntity? Player => Connection.player;

        public MinecraftProtocol ProtocolVersion = MinecraftProtocol.Base;
        public string ClientBrand = string.Empty;
        public object? StateContext;

        public int TicksSinceKeepAlive;
        public int TicksSinceKeepAliveResponse;
        public ServerboundClientInformation ClientInformation = new();

        public GameProfile Profile = new();

        public void SwitchState(NetworkState state)
            => Connection.SwitchState(state);

        public void EnableCompression(int threshold)
            => Connection.EnableCompression(threshold);

        public void EnableEncryption(byte[] sharedSecret)
            => Connection.EnableEncryption(sharedSecret);

        public void AllocatePlayer()
        {
            if(Player == null)
            {
                Connection.player = new ServerPlayerEntity(this, Profile);
            }
        }
    }

    private static ConnectionPacketHandler<ServerConnectionContext> ServerConnectionPacketHandler = new((new Dictionary<NetworkState, PacketStateHandler<ServerConnectionContext>>()
            {
                { NetworkState.Handshake, new HandshakeServerStateHandler() },
                { NetworkState.Status, new StatusServerStateHandler() },
                { NetworkState.Login, new LoginServerStateHandler() },
                { NetworkState.Configuration, new ConfigurationServerStateHandler() },
                { NetworkState.Play, new PlayServerStateHandler() }
            }).ToFrozenDictionary());

    public static readonly TextPart StatusSentSuccesfully = TextPart.String("Status response sent succesfully");
    private static ReadOnlySpan<byte> ValidNextStates => [(byte)NetworkState.Status, (byte)NetworkState.Login, (byte)NetworkState.Transfer];
    
    private readonly ServerConnectionContext context;
    private readonly ServerNetworkGameManager gameHandler;
    private readonly RSAKeyring serverKeyring;
    private ServerPlayerEntity? player = null;

    public ServerNetworkGameManager GameHandler
        => this.gameHandler;

    public ServerPlayerEntity? Player
        => this.player;

    public ServerNetworkConnection(ConnectionContext context, ServerNetworkGameManager gameHandler, RSAKeyring serverKeyring, ILoggerFactory logger) : base(context, ServerConnectionPacketHandler, logger)
    {
        this.gameHandler = gameHandler;
        this.serverKeyring = serverKeyring;
        this.context = new(this);
    }

    protected override ServerConnectionContext ProvideHandlerContext()
        => this.context;
}
