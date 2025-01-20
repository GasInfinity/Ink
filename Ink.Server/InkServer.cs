using Ink.Server.Event;
using Ink.Server.Net;
using Ink.Server.World;
using Ink.Util;
using Microsoft.Extensions.Logging;

namespace Ink.Server;

public sealed class InkServer : IAsyncTickable
{
    public const int TicksPerSecond = 20;
    public const int MillisecondsPerTick = 1000 / 20;

    private readonly CancellationTokenSource stopSource = new();
    private ThreadSafeFlag tickingFlag = new();

    private readonly ServerNetworkGameManager connectionGameHandler;
    public readonly ServerNetworkManager ConnectionHandler;
    public readonly ServerRegistryManager RegistryManager;
    public readonly ServerWorldManager WorldManager;

    public int MaxGlobalViewDistance { get; set; } = 16;
    public IAsyncPreLoginListener? PreloginListener { get; init; }

    public InkServer(InkServerOptions options, ILoggerFactory factory)
    {
        RegistryManager = new();
        WorldManager = new(RegistryManager);

        this.connectionGameHandler = new(RegistryManager, options.LoginListener);
        ConnectionHandler = new ServerNetworkManager(this.connectionGameHandler, options.NetworkOptions, factory);
    }

    public async Task RunAsync()
    {
        await Task.WhenAll(ConnectionHandler.AcceptClientsAsync(stopSource.Token), LoopAsync());
    }

    public ValueTask TickAsync()
    {
        if (!this.tickingFlag.TrySet())
            ThrowHelpers.ThrowTickingWhileTicked();

        try
        {
            ConnectionHandler.Tick();

            this.connectionGameHandler.Tick();
            WorldManager.Tick();
        }
        catch(Exception e)
        {
            Console.WriteLine($"Exception while ticking {e}!");
        }
        finally
        {
            this.tickingFlag.Reset();
        }

        return ValueTask.CompletedTask;
    }

    private async Task LoopAsync()
    {
        try
        {
            PeriodicTimer tickTimer = new(TimeSpan.FromMilliseconds(MillisecondsPerTick));
            while (await tickTimer.WaitForNextTickAsync(this.stopSource.Token))
            {
                await TickAsync();
            }
        }
        catch(OperationCanceledException)
        {
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Tick exception !!!!! {ex}");
        }
    }
}
