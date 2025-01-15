using Ink.Math;
using Ink.Server.Entities;

namespace Ink.Server.Net;

public sealed class ServerPlayerChunkManager
{
    private readonly HashSet<ChunkPosition> newChunks = new();
    private readonly HashSet<ChunkPosition> unloadedChunks = new();
    private HashSet<ChunkPosition> lastViewingChunks = new();
    private HashSet<ChunkPosition> viewingChunks = new();

    public IEnumerable<ChunkPosition> NewChunks
        => this.newChunks;

    public IEnumerable<ChunkPosition> UnloadedChunks
        => this.unloadedChunks;

    public ServerPlayerChunkManager()
    {
    }

    public bool IsViewing(ChunkPosition position)
        => this.lastViewingChunks.Contains(position);

    // TODO: Refactor this
    public void UpdateChunks(ServerPlayerEntity player)
    {
        ChunkPosition position = ((BlockPosition)player.Position).ToChunkPosition();

        int view = player.ViewDistance;
        int startX = position.X - view;
        int endX = position.X + view;
        int startZ = position.Z - view;
        int endZ = position.Z + view;

        this.viewingChunks.Clear();
        this.newChunks.Clear();
        this.unloadedChunks.Clear();

        for (int x = startX; x <= endX; ++x) // TODO Spiral enqueue
        {
            for (int z = startZ; z <= endZ; ++z)
            {
                ChunkPosition current = new(x, z);

                if(this.viewingChunks.Add(current)
                && !this.lastViewingChunks.Contains(current))
                {
                    this.newChunks.Add(current);
                }
            }
        }

        foreach(ChunkPosition lastPosition in this.lastViewingChunks)
        {
            if(!this.viewingChunks.Contains(lastPosition))
            {
                this.unloadedChunks.Add(lastPosition);
            }
        }

        (this.lastViewingChunks, this.viewingChunks) = (this.viewingChunks, this.lastViewingChunks);
    }
}
