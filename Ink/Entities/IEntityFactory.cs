using Ink.Util;

namespace Ink.Entities;

public interface IEntityFactory<TEntity>
    where TEntity : Entity, IEntityFactory<TEntity>
{
    static abstract TEntity Create(IEntityTrackerFactory trackerFactory);
    static abstract TEntity Create(Uuid uuid, IEntityTrackerFactory trackerFactory);
    static abstract TEntity Create(int entityId, Uuid uuid, IEntityTrackerFactory trackerFactory);
}
