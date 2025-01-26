using Friflo.Engine.ECS;
using Ink.Math;

namespace Ink.Server.Entities.Components;

public record struct EntityLastSyncedSpawnPositionComponent(BlockPosition Location, float Yaw) : IComponent
{
    public BlockPosition Location = Location;
    public float Yaw = Yaw;
}
