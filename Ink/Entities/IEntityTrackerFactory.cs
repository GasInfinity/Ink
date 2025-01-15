namespace Ink.Entities;

public interface IEntityTrackerFactory
{
    IEntityTracker Create(Entity entity);
}
