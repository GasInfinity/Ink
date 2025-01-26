using Ink.Server;
using Ink.Worlds;
using Ink.Chunks;
using Ink.Registries;
using Ink.Chat;
using Ink.Worlds.Biomes;
using Ink.Event.Registry;
using Ink.Entities.Damage;
using System.Text.Json;
using Ink.Util;
using Ink.Server.Worlds;
using Ink.Server.Event;
using System.Net;
using Microsoft.Extensions.Logging;
using Ink.Items;
using Ink.Blocks;
using Ink.Vanilla.Blocks;
using Ink.Vanilla.Items;
using Ink.Entities;
using System.Runtime;
using Friflo.Engine.ECS;

class Server : ILoginListener, IValueRegistrationListener<Item>, IValueRegistrationListener<Block>, IValueRegistrationListener<BiomeType>, IValueRegistrationListener<DimensionType>, IValueRegistrationListener<ChatType>, IValueRegistrationListener<DamageType>, IValueRegistrationListener<WolfVariant>, IValueRegistrationListener<PaintingVariant>
{
    static readonly StateStorage Air = new (0);
    static readonly StateStorage Test = new (1);

    static async Task Main()
    {
        int newWorkerMin = Math.Max(Environment.ProcessorCount * 4, 32);
        ThreadPool.SetMinThreads(newWorkerMin, newWorkerMin);
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
        Server server = new();
        await server.RunAsync();
    }

    private readonly InkServer server;
    private readonly ServerWorld world;

    public Server()
    {
        Console.WriteLine($"PID {Environment.ProcessId}");
        this.server = new(new()
        {
            LoginListener = this,
            NetworkOptions = new()
            {
                ListeningEndPoint = new IPEndPoint(IPAddress.Any, 25565),
            }
        }, LoggerFactory.Create(c => c.AddSimpleConsole(c =>
        {
            c.SingleLine = true;
            c.IncludeScopes = true;
            c.TimestampFormat = "dd-MM-yy@HH:mm:ss ";
        }).SetMinimumLevel(LogLevel.Trace)));

        ServerRegistryManager serverRegistryManager = server.RegistryManager;
        serverRegistryManager.ItemRegistryListener = this;
        serverRegistryManager.BlockRegistryListener = this;
        serverRegistryManager.BiomeRegistryListener = this;
        serverRegistryManager.ChatTypeRegistryListener = this;
        serverRegistryManager.DimensionRegistryListener = this;
        serverRegistryManager.DamageTypeRegistryListener = this;
        serverRegistryManager.WolfVariantRegistryListener = this;
        serverRegistryManager.PaintingVariantRegistryListener = this;
        serverRegistryManager.RegisterFreeze();

        ServerWorldManager worldManager = this.server.WorldManager;
        this.world = worldManager.CreateWorld(Identifier.Vanilla("test"));
    }

    public Task RunAsync()
        => this.server.RunAsync();

    public void OnLogin(ref LoginEvent loginEvent)
    {
        loginEvent.AssignedWorld = this.world;
    }

    public void OnRegistration(ValueRegistrationEvent<Block> registrationEvent)
    {
        VanillaBlocks.RegisterAll(registrationEvent.Registry);
    }

    public void OnRegistration(ValueRegistrationEvent<Item> registrationEvent)
    {
        VanillaItems.RegisterAll(registrationEvent.Registry);
    }

    public void OnRegistration(ValueRegistrationEvent<ChatType> registrationEvent)
    {
        FrozenRegistryBuilder<ChatType> registry = registrationEvent.Registry;

        registry.Register(Identifier.Vanilla("chat"), ChatType.Default);
    }

    public void OnRegistration(ValueRegistrationEvent<DimensionType> registrationEvent)
    {
        FrozenRegistryBuilder<DimensionType> registry = registrationEvent.Registry;

        registry.Register(Identifier.Vanilla("test"), DimensionType.Overworld with
        {
            MinY = 0,
            Height = 16,
            LogicalHeight = 16
        });
    }

    public void OnRegistration(ValueRegistrationEvent<BiomeType> registrationEvent)
    {
        FrozenRegistryBuilder<BiomeType> registry = registrationEvent.Registry;

        registry.Register(Identifier.Vanilla("plains"), new()
        {
            HasPrecipitation = false,
            Temperature = 0.8f,
            Downfall = 0.4f,
            Effects = new()
            {
                FogColor = 12638463,
                SkyColor = 7907327,
                WaterColor = 4159204,
                WaterFogColor = 329011
            }
        });
    }

    public void OnRegistration(ValueRegistrationEvent<PaintingVariant> registrationEvent)
    {
        FrozenRegistryBuilder<PaintingVariant> registry = registrationEvent.Registry;

        registry.Register(Identifier.Vanilla("painting"), new PaintingVariant(Identifier.Vanilla("a")));
    }

    public void OnRegistration(ValueRegistrationEvent<WolfVariant> registrationEvent)
    {
        FrozenRegistryBuilder<WolfVariant> registry = registrationEvent.Registry;
        registry.Register(Identifier.Vanilla("wolf"), new WolfVariant(Identifier.Vanilla("a"), Identifier.Vanilla("a"), Identifier.Vanilla("a")));
    }

    public void OnRegistration(ValueRegistrationEvent<DamageType> registrationEvent)
    {
        FrozenRegistryBuilder<DamageType> registry = registrationEvent.Registry;

        foreach (string damagePath in Directory.EnumerateFiles("damage_types"))
        {
            string damageType = Path.GetFileNameWithoutExtension(damagePath);
            using FileStream stream = File.OpenRead(damagePath);
            DamageType damage = JsonSerializer.Deserialize(stream, InkJsonContext.Default.DamageType)!;
            registry.Register(Identifier.Vanilla(damageType), damage);
        }
    }
}
