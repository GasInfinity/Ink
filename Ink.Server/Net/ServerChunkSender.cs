using Ink.Math;
using Ink.World;
using Ink.Net.Packets.Play;
using Rena.Native.Buffers;
using System.Buffers;
using Ink.Chunks;
using Ink.Util.Extensions;
using Ink.Nbt.Tags;

namespace Ink.Server.Net;

// TODO: This does not work correctly, how does minecraft handle batches? I cannot find info...
public sealed class ServerChunkSender
{
    private readonly HashSet<ChunkPosition> sentChunks = new();
    private readonly HashSet<ChunkPosition> chunks = new();

    private BaseWorld? currentWorld;
    
    private float desiredChunksPerTick = 8f;
    private float chunkTickBuffer = 0;

    private int maxUnacknowledgedBatches = 1;
    private int unacknowledgedBatches = 0;

    public BaseWorld? CurrentWorld
    { 
        get => this.currentWorld;
        set 
        {
            this.currentWorld = value;
            ResetChunkBatchState();
        }
    }

    public ServerChunkSender()
    {
    }

    public void AddChunk(ChunkPosition position)
    {
        if(!this.chunks.Add(position))
        {
            Console.WriteLine($"WHAT? ADDING SAME CHUNK TWICE?? (Wasting bandwidth)"); 
        }
    }

    public void RemoveChunk(ChunkPosition position, ServerNetworkConnection connection)
    {
        if(!this.chunks.Remove(position))
        {
            connection.Send(new ClientboundForgetLevelChunk(position.Z, position.X));
        }
    }

    public void SendChunks(ServerNetworkConnection connection)
    {
        if(this.unacknowledgedBatches >= this.maxUnacknowledgedBatches)
            return;

        if(this.chunks.Count <= 0) 
            return;

        this.chunkTickBuffer += this.desiredChunksPerTick;
        int batchSize = int.Min((int) this.chunkTickBuffer, this.chunks.Count);
        this.chunkTickBuffer -= batchSize;

        connection.Send(new ClientboundChunkBatchStart());
        using PooledArrayBufferWriter<byte> chunkWriter = new(ArrayPool<byte>.Shared);
        using PooledArrayBufferWriter<byte> lightWriter = new(ArrayPool<byte>.Shared);
        int currentSentChunk = 0;
        foreach(ChunkPosition position in this.chunks)
        {
            // FIXME: this is badd.....
            ref Chunk chunk = ref this.currentWorld!.GetChunk(position);

            chunkWriter.WriteNbt(new RootTag(NbtTag.Compound(new(){ { "MOTION_BLOCKING", NbtTag.LongArray([]) } }), null));
            chunk.Write(chunkWriter);
            chunk.WriteLight(lightWriter);

            connection.Send(new ClientboundLevelChunkWithLight(position.X, position.Z, new(chunkWriter.WrittenMemory), new(lightWriter.WrittenMemory)));
            chunkWriter.Reset();
            lightWriter.Reset();

            this.sentChunks.Add(position);
            ++currentSentChunk;
        }

        this.chunks.ExceptWith(this.sentChunks);
        this.sentChunks.Clear();

        connection.Send(new ClientboundChunkBatchFinished(batchSize));
        this.unacknowledgedBatches++;
    }

    public void AcknowledgeChunkBatch(float chunksPerTick)
    {
        if(this.maxUnacknowledgedBatches == 1)
        {
            this.maxUnacknowledgedBatches = 10;
        }

        if(this.unacknowledgedBatches > 0)
        {
            this.unacknowledgedBatches--;
        }

        this.desiredChunksPerTick = float.Min(chunksPerTick, 1);
    }

    public void ResetChunkBatchState()
    {
        this.maxUnacknowledgedBatches = 1;
        this.unacknowledgedBatches = 0;
        this.desiredChunksPerTick = 8;
    }
}
