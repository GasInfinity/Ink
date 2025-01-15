using System.Collections.Concurrent;
using Ink.Text;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ink.Net.Encryption;

namespace Ink.Server.Net;

public sealed class ServerNetworkManager : ITickable, IAsyncDisposable
{
    private static readonly TextPart InternalServerError = TextPart.String("Internal Server Error: Contact the administrator!");

    const long KeepAliveInterval = 15000; // Every 15 seconds (make configurable?)
    const long MaxKeepAliveResponseTime = 30000; // If the client does not respond for 30 seconds, disconnect (make configurable?)

    private readonly ServerNetworkGameManager gameHandler;
    private readonly InkServerOptions.Network options;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<ServerNetworkManager> logger;
    private readonly SocketTransportFactory transportFactory;
    private readonly RSAKeyring serverKeyring;

    private readonly ConcurrentDictionary<ServerNetworkConnection, Task> establishedConnections;

    private IConnectionListener? connectionListener;

    public ServerNetworkManager(ServerNetworkGameManager gameHandler, InkServerOptions.Network options, ILoggerFactory loggerFactory)
    {
        this.gameHandler = gameHandler;
        this.options = options;
        this.loggerFactory = loggerFactory;
        this.logger = loggerFactory.CreateLogger<ServerNetworkManager>();
        this.transportFactory = new (Options.Create(new SocketTransportOptions()), loggerFactory);
        this.serverKeyring = new(ServerConstants.RsaKeypairSize);
        this.establishedConnections = new();
    }

    public async Task AcceptClientsAsync(CancellationToken shutdownToken)
    {
        this.logger.LogInformation("Started listening on {EndPoint}", options.ListeningEndPoint);
        this.connectionListener = await this.transportFactory.BindAsync(options.ListeningEndPoint, shutdownToken);

        while (!shutdownToken.IsCancellationRequested)
        {
            try
            {
                ConnectionContext? context = await this.connectionListener.AcceptAsync(shutdownToken);

                if(context == null)
                    break;

                ServerNetworkConnection connection = new(context, this.gameHandler, this.serverKeyring, this.loggerFactory);
                _ = this.establishedConnections.TryAdd(connection, HandleConnectionAsync(connection));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception while accepting clients: {e}");
            }
        }
    }

    private async Task HandleConnectionAsync(ServerNetworkConnection connection)
    {
        await connection.HandleConnectionAsync();

        if(this.establishedConnections.TryRemove(connection, out _))
            await connection.DisposeAsync();
    }

    public void Tick()
    {
        foreach((ServerNetworkConnection connection, Task handlingTask) in this.establishedConnections)
        {
            if(handlingTask.IsFaulted)
            {
                connection.Disconnect(InternalServerError);
                continue;
            }

            if(connection.IsConnected)
                connection.Tick();
        }
    }
    
    public ValueTask DisposeAsync()
        => this.connectionListener?.DisposeAsync() ?? ValueTask.CompletedTask;
}
