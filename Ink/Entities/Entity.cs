using Friflo.Engine.ECS;
using Ink.Auth;
using Ink.Entities.Components;
using Ink.Util;
using Ink.Worlds;
using Rena.Mathematics;

namespace Ink.Entities;

// TODO: Implement physics and metadata again and think about it...
public readonly record struct InkEntity(Entity Entity)
{
    private static int CurrentNetworkId = -1;
    private static int NextNetworkId => Interlocked.Increment(ref CurrentNetworkId);

    public readonly Entity Entity = Entity;

    public int NetworkId => Entity.GetComponent<EntityIdComponent>().NetworkId;
    public Uuid Id => Entity.GetComponent<EntityIdComponent>().Id;

    public ref EntityTransformComponent Transform
        => ref Entity.GetComponent<EntityTransformComponent>();

    public ref Vec3<double> Position
        => ref Entity.GetComponent<EntityTransformComponent>().Position;

    public ref Vec3<double> Velocity 
        => ref Entity.GetComponent<EntityTransformComponent>().Velocity;

    public ref Vec2<float> Rotation 
        => ref Entity.GetComponent<EntityTransformComponent>().Rotation;

    public ref float HeadYaw 
        => ref Entity.GetComponent<EntityTransformComponent>().HeadYaw;

    public static CreateEntityBatch Create(CreateEntityBatch batch, Uuid uuid)
        => batch.Add(new EntityIdComponent(NextNetworkId, uuid))
                .Add(new EntityTransformComponent())
                .Add(new EntityLastTransformComponent());
}

public readonly record struct LivingEntity(Entity Entity)
{
    public readonly Entity Entity = Entity;

    public ref float Health
        => ref Entity.GetComponent<EntityHealthComponent>().Health;

    public InkEntity Base
        => new(Entity);

    public static CreateEntityBatch Create(CreateEntityBatch batch, Uuid uuid)
        => InkEntity.Create(
                batch.Add(new EntityHealthComponent()),
                uuid
        );
}

public readonly record struct PlayerEntity(Entity Entity)
{
    public readonly Entity Entity = Entity;

    public ref readonly GameProfile Profile
        => ref Entity.GetComponent<EntityPlayerComponent>().Profile;

    public ref GameMode CurrentGameMode
        => ref Entity.GetComponent<EntityPlayerComponent>().CurrentGameMode;

    // TODO: Other ones

    public LivingEntity Living 
        => new(Entity);

    public static CreateEntityBatch Create(CreateEntityBatch batch, GameProfile profile)
        => LivingEntity.Create(
                batch.Add(new EntityPlayerComponent(profile))
                     .Add(new EntitySpawnPositionComponent()),
                profile.Id
        );
}
