using Friflo.Engine.ECS;
using Ink.Math;

namespace Ink.Server.Entities.Components;

public record struct EntityChunkSenderComponent(float DesiredChunksPerTick = 8, float ChunkTickBuffer = 0, int MaxUnacknowledgedBatches = 1, int UnacknowledgedBatches = 0) : IComponent
{
    public readonly HashSet<ChunkPosition> SendQueue = new();
    public readonly HashSet<ChunkPosition> SentChunks = new();

    public float DesiredChunksPerTick = DesiredChunksPerTick;
    public float ChunkTickBuffer = ChunkTickBuffer;

    public int MaxUnacknowledgedBatches = MaxUnacknowledgedBatches;
    public int UnacknowledgedBatches = UnacknowledgedBatches;
}
