using Friflo.Engine.ECS;
using Ink.Math;

namespace Ink.Entities.Components;

public record struct EntitySpawnPositionComponent(BlockPosition Location, float Yaw) : IComponent
{
    public BlockPosition Location = Location;
    public float Yaw = Yaw;
}
