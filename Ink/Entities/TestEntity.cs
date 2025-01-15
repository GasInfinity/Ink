using Ink.Util;

namespace Ink.Entities;

public class TestEntity : Entity, IEntityFactory<TestEntity>
{
    public TestEntity(int entityId, Uuid uuid, IEntityTrackerFactory factory) : base(entityId, uuid, EntityDefinition.Cow, factory)
    {
    }

    public TestEntity(Uuid uuid, IEntityTrackerFactory factory) : base(uuid, EntityDefinition.Cow, factory)
    {
    }

    public TestEntity(IEntityTrackerFactory factory) : base(EntityDefinition.Cow, factory)
    {
    }

    protected override void TickLogic()
    {
        base.Tick();
    }

    public static TestEntity Create(IEntityTrackerFactory trackerFactory)
        => new(trackerFactory);

    public static TestEntity Create(Uuid uuid, IEntityTrackerFactory trackerFactory)
        => new(uuid, trackerFactory);

    public static TestEntity Create(int entityId, Uuid uuid, IEntityTrackerFactory trackerFactory)
        => new(entityId, uuid, trackerFactory);
}
