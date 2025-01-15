using Ink.Registries;
using Ink.Util;
using System.Collections.Concurrent;

namespace Ink.Server.World
{
    public sealed class ServerWorldManager : ITickable
    {
        private readonly ServerRegistryManager registryManager;
        private readonly ConcurrentDictionary<Uuid, ServerWorld> worlds = new ConcurrentDictionary<Uuid, ServerWorld>();

        private ThreadSafeFlag tickingFlag = new();

        public ServerWorldManager(ServerRegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        public ServerWorld CreateWorld(Identifier dimensionName)
            => new(registryManager, registryManager.Block, registryManager.DimensionType, dimensionName);

        public ServerWorld CreateWorld(Uuid uuid, Identifier dimensionName)
            => new (uuid, registryManager, registryManager.Block, registryManager.DimensionType, dimensionName);

        public void RegisterWorld(ServerWorld world)
        {
            _ = this.worlds.TryAdd(world.Uuid, world);
        }

        public void UnregisterWorld(Uuid worldUuid)
        {
            _ = this.worlds.TryRemove(worldUuid, out _);
        }

        public void UnregisterWorld(ServerWorld world)
        {
            _ = this.worlds.TryRemove(world.Uuid, out _);
        }

        public void Tick()
        {
            if (!this.tickingFlag.TrySet())
                ThrowHelpers.ThrowTickingWhileTicked();

            try
            {
                foreach (var world in worlds.Values)
                {
                    world.Tick();
                }
            }
            finally
            {
                this.tickingFlag.Reset();
            }
        }
    }
}
