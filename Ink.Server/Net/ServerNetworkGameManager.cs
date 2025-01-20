using System.Collections.Concurrent;
using ConcurrentCollections;
using Ink.Net;
using Ink.Server.Entities;
using Ink.Server.Event;
using Ink.Net.Packets.Play;
using Ink.Text;
using Ink.Util;

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

    public void AddPlaying(ServerNetworkConnection connection)
    {
        this.playingLimboConnections.Add(connection);
    }

    public void RemovePlaying(ServerNetworkConnection connection)
    {
        if(this.playingConnections.TryRemove(connection))
        {
            this.stoppedPlayingConnections.Add(connection);
        }
    }

    public void BroadcastPlay<TPacket>(TPacket packet)
        where TPacket : struct, IPacket<TPacket>
    {
        foreach (ServerNetworkConnection connection in this.playingConnections)
        {
            connection.Send(packet);
        }
    }

    public void BroadcastPlayExcept<TPacket>(TPacket packet, ServerNetworkConnection except)
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

            ServerPlayerEntity? playerEntity = connection.Player!;
            LoginEvent e = new(playerEntity);

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

            playerEntity.Initialize(e.AssignedWorld);
            e.AssignedWorld.AddEntity(playerEntity);
            ++handledPlayers;
        }
    }

    private void HandleDisconnectedPlayers()
    {
        if (this.stoppedPlayingConnections.IsEmpty)
            return;

        List<Uuid> playersRemoved = new List<Uuid>(this.stoppedPlayingConnections.Count);

        while(this.stoppedPlayingConnections.TryTake(out ServerNetworkConnection? connection))
        {
            ServerPlayerEntity player = connection.Player!;
            player.Remove();

            playersRemoved.Add(player.Uuid);
        }

        BroadcastPlay(new ClientboundPlayerInfoRemove(playersRemoved.ToArray()));
    }
}
