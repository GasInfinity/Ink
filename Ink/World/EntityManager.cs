using Ink.Entities;
using Ink.Math;
using Ink.Util;

namespace Ink.World;

public sealed class EntityManager : ITickable
{
    private readonly Dictionary<Uuid, Entity> allEntities = new();

    public IEnumerable<Entity> Entities
        => this.allEntities.Values;

    public void AddEntity(Entity entity)
    {
        if(this.allEntities.TryAdd(entity.Uuid, entity))
        {
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if(this.allEntities.Remove(entity.Uuid))
        {
        }
    }
    
    public IEnumerable<Entity> NearbyEntities(BlockPosition position)
    {
        return this.allEntities.Values;
    }

    public void Tick()
    {
        foreach(Entity entity in this.allEntities.Values)
        {
            entity.Tick(); 
        }
    }
}
