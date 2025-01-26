using System.Collections.Concurrent;
using ConcurrentCollections;
using Ink.Net;
using Ink.Server.Entities;
using Ink.Server.Event;
using Ink.Net.Packets.Play;
using Ink.Text;
using Ink.Util;
using Ink.Worlds;
using Ink.Net.Structures;

namespace Ink.Server.Net;

public sealed class ServerNetworkGameManager : ITickable
{
    const int MaxWaitingPlayersPerTick = 100;

    private static readonly TextPart NoWorldPart = TextPart.String("No world specified on login");

    private readonly ConcurrentHashSet<ServerNetworkConnection> playingConnections = [];
    private readonly ConcurrentBag<ServerNetworkConnection> playingLimboConnections = [];
    private readonly ConcurrentBag<ServerNetworkConnection> stoppedPlayingConnections = [];

    private readonly ServerRegistryManager registryManager;
    private readonly ILoginListener loginListener;

    public ServerRegistryManager RegistryManager
        => this.registryManager;

    public int CurrentlyPlaying
        => this.playingConnections.Count;

    public ServerNetworkGameManager(ServerRegistryManager registryManager, ILoginListener loginListener)
    {
        this.registryManager = registryManager;
        this.loginListener = loginListener;
    }

    public void QueuePlaying(ServerNetworkConnection context)
    {
        this.playingLimboConnections.Add(context);
    }

    public void RemovePlaying(ServerNetworkConnection connection)
    {
        if(this.playingConnections.TryRemove(connection))
        {
            this.stoppedPlayingConnections.Add(connection);
        }
    }

    public void Broadcast<TPacket>(TPacket packet)
        where TPacket : struct, IPacket<TPacket>
    {
        foreach (ServerNetworkConnection connection in this.playingConnections)
        {
            connection.Send(packet);
        }
    }

    public void BroadcastExcept<TPacket>(TPacket packet, ServerNetworkConnection except)
        where TPacket : struct, IPacket<TPacket>
    {
        foreach (ServerNetworkConnection connection in this.playingConnections)
        {
            if (connection == except)
                continue;

            connection.Send(packet);
        }
    }

    public void Tick()
    {
        HandleWaitingPlayers();
        HandleDisconnectedPlayers();
    }

    private void HandleWaitingPlayers()
    {
        int handledPlayers = 0;
        while (handledPlayers < MaxWaitingPlayersPerTick
            && this.playingLimboConnections.TryTake(out ServerNetworkConnection? connection))
        {
            if (!connection.IsConnected)
                continue;

            // ServerPlayerEntity? playerEntity = connection.Player!;
            LoginEvent e = new();

            this.loginListener.OnLogin(ref e);

            if (!connection.IsConnected) // Maybe kicked?
                continue;

            if(e.AssignedWorld == null)
            {
                connection.Disconnect(NoWorldPart);
                continue;
            }

            if(!this.playingConnections.Add(connection))
                continue; // FIXME: Throw exception, this should NEVER happen?

            // playerEntity.Initialize(e.AssignedWorld);
            RemotePlayerEntity remotePlayer = e.AssignedWorld.SpawnRemotePlayerEntity(connection);
            remotePlayer.Player.CurrentGameMode = GameMode.Creative;
            connection.AssignPlayer(remotePlayer);

            // TODO: Move this to another method or something, there's something off about this class...
            connection.Send(new ClientboundLogin(
                EntityId: remotePlayer.Player.Living.Base.NetworkId,
                IsHardcore: false,
                DimensionNames: RegistryManager.DimensionType.Keys.ToArray(),
                MaxPlayers: 0,
                ViewDistance: remotePlayer.ViewDistance,
                SimulationDistance: 4,
                ReducedDebugInfo: false,
                EnableRespawnScreen: true,
                DoLimitedCrafting: false,
                DimensionType: RegistryManager.DimensionType.GetId(e.AssignedWorld.Dimension),
                DimensionName: e.AssignedWorld.Dimension,
                HashedSeed: 0,
                GameMode: remotePlayer.Player.CurrentGameMode,
                PreviousGameMode: GameMode.Undefined,
                IsDebug: false,
                IsFlat: false,
                HasDeathLocation: false,
                DeathDimensionName: null,
                DeathLocation: null,
                PortalCooldown: 0,
                SeaLevel: 64,
                EnforcesSecureChat: false
            ));

            connection.Send(new ClientboundGameEvent(13, 0)); // TODO: Enum (StartWaitingForLevelChunks)

            // TODO: Make this configurable, should we broadcast inside the world or serverwide? etc...
            Broadcast(new ClientboundPlayerInfoUpdate(new PlayersInfo(
                Actions: PlayersInfo.Action.AddPlayer | PlayersInfo.Action.UpdateListed | PlayersInfo.Action.UpdateGameMode,
                Players: [new PlayersInfo.Info(Profile: remotePlayer.Player.Profile, Listed: true, GameMode: remotePlayer.Player.CurrentGameMode)]
            )));

            // FIXME: This is temporal, move to a non allcating solution
            PlayersInfo.Info[] allConnected = new PlayersInfo.Info[this.playingConnections.Count - 1];
            int i = 0;
            foreach(ServerNetworkConnection otherConnection in this.playingConnections)
            {
                if(connection == otherConnection)
                    continue;

                RemotePlayerEntity otherRemotePlayer = otherConnection.Player;
                allConnected[i] = new PlayersInfo.Info(Profile: otherRemotePlayer.Player.Profile, Listed: true, GameMode: otherRemotePlayer.Player.CurrentGameMode);
            }

            connection.Send(new ClientboundPlayerInfoUpdate(new PlayersInfo(
                Actions: PlayersInfo.Action.AddPlayer | PlayersInfo.Action.UpdateListed | PlayersInfo.Action.UpdateGameMode,
                Players: allConnected
            )));

            ++handledPlayers;
        }
    }

    private void HandleDisconnectedPlayers()
    {
        if (this.stoppedPlayingConnections.IsEmpty)
            return;

        Uuid[] playersRemoved = new Uuid[this.stoppedPlayingConnections.Count];

        int i = 0;
        while(i < playersRemoved.Length
        && this.stoppedPlayingConnections.TryTake(out ServerNetworkConnection? connection))
        {
            RemotePlayerEntity remotePlayer = connection.Player;
            playersRemoved[i] = remotePlayer.Player.Living.Base.Id;
            remotePlayer.Entity.Enabled = false;
        }

        Broadcast(new ClientboundPlayerInfoRemove(playersRemoved.ToArray()));
    }
}
