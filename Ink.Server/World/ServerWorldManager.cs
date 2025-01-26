using Friflo.Engine.ECS.Systems;
using Ink.Registries;
using Ink.Server.Worlds.Systems;
using Ink.Util;
using Ink.Worlds;
using System.Collections.Concurrent;

namespace Ink.Server.Worlds
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
            => CreateWorld((Uuid)Guid.NewGuid(), dimensionName);

        public ServerWorld CreateWorld(Uuid uuid, Identifier dimensionName)
        {
            DimensionType dimensionType = registryManager.DimensionType.Get(dimensionName)
                            ?? throw new ArgumentException($"Dimension '{dimensionName}' not found inside dimensions registry");

            ServerChunkManager chunkManager = new(dimensionType);
            ServerWorld world = new (uuid, chunkManager, registryManager, registryManager.Block, registryManager.DimensionType, dimensionName);

            if(!this.worlds.TryAdd(uuid, world))
            {
                throw new NotSupportedException(); // TODO: Duplicated uuid
            }

            return world;
        }

        public void Tick()
        {
            if (!this.tickingFlag.TrySet())
                ThrowHelpers.ThrowTickingWhileTicked();

            try
            {
                // TODO: Research if all worlds could be ticked in pararell
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
