using System.Buffers;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Chunks;
using Ink.Math;
using Ink.Nbt.Tags;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;
using Ink.Server.Net;
using Ink.Util;
using Ink.Util.Extensions;
using Rena.Native.Buffers;

namespace Ink.Server.Worlds.Systems;

public sealed class ChunkSenderSystem(ServerWorld world) : QuerySystem<EntityRemotePlayerComponent, EntityChunkViewerComponent, EntityChunkSenderComponent>
{
    private readonly ServerWorld world = world;

    protected override void OnUpdate()
    {
        Query.Each(new SendChunkEach(this.world));
    }

    private static readonly RootTag MotionBlockingDummy = new RootTag(NbtTag.Compound(new(){ { "MOTION_BLOCKING", NbtTag.LongArray([]) } }), null);
    private readonly struct SendChunkEach(ServerWorld world) : IEach<EntityRemotePlayerComponent, EntityChunkViewerComponent, EntityChunkSenderComponent>
    {
        private readonly ServerWorld world = world;

        public void Execute(ref EntityRemotePlayerComponent remote, ref EntityChunkViewerComponent viewer, ref EntityChunkSenderComponent sender)
        {
            ServerNetworkConnection connection = remote.Connection;

            foreach(ChunkPosition position in viewer.Old)
            {
                if(!sender.SendQueue.Remove(position))
                    connection.Send(new ClientboundForgetLevelChunk(position.Z, position.X));
            }

            foreach(ChunkPosition position in viewer.New)
            {
                _ = sender.SendQueue.Add(position);
            }

            if(sender.UnacknowledgedBatches >= sender.MaxUnacknowledgedBatches)
                return;

            if(sender.SendQueue.Count <= 0)
                return;

            sender.ChunkTickBuffer += sender.DesiredChunksPerTick;
            int batchSize = int.Min((int) sender.ChunkTickBuffer, sender.SendQueue.Count);
            sender.ChunkTickBuffer -= batchSize;

            connection.Send(new ClientboundChunkBatchStart());
            // FIXME: this is badd.....
            using PooledArrayBufferWriter<byte> chunkWriter = Utilities.SharedBufferWriters.Get();
            using PooledArrayBufferWriter<byte> lightWriter = Utilities.SharedBufferWriters.Get();
            sender.SentChunks.Clear();
            foreach(ChunkPosition position in sender.SendQueue)
            {
                if(sender.SentChunks.Count == batchSize)
                    break;

                // FIXME: For now this will never fail. before beggining to send we need to know which chunks are loaded and send ONLY those ones that are
                ref Chunk chunk = ref this.world.ChunkManager.GetChunkRefOrNullRef(position);

                chunkWriter.WriteNbt(MotionBlockingDummy);
                chunk.Write(chunkWriter);
                chunk.WriteLight(lightWriter);

                connection.Send(new ClientboundLevelChunkWithLight(position.X, position.Z, new(chunkWriter.WrittenMemory), new(lightWriter.WrittenMemory)));
                chunkWriter.Reset();
                lightWriter.Reset();

                sender.SentChunks.Add(position);
            }

            Utilities.SharedBufferWriters.Return(chunkWriter);
            Utilities.SharedBufferWriters.Return(lightWriter);

            sender.SendQueue.ExceptWith(sender.SentChunks);
            connection.Send(new ClientboundChunkBatchFinished(batchSize));
            sender.UnacknowledgedBatches++;
        }
    }
}
