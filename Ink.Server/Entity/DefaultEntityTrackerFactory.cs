using Ink.Entities;

namespace Ink.Server.Entities;

public class DefaultEntityTrackerFactory : IEntityTrackerFactory
{
    public static readonly DefaultEntityTrackerFactory Shared = new();

    public IEntityTracker Create(Entity entity)
        => new EntityTracker(entity);
}
