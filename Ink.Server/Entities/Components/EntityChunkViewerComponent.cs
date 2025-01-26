using Friflo.Engine.ECS;
using Ink.Math;

namespace Ink.Server.Entities.Components;

public record struct EntityChunkViewerComponent(int Distance = 2) : IComponent
{
    public readonly HashSet<ChunkPosition> New = new();
    public readonly HashSet<ChunkPosition> Old = new();
    public HashSet<ChunkPosition> Viewing = new();
    public HashSet<ChunkPosition> LastViewing = new();

    public int Distance = Distance;
}
