using Friflo.Engine.ECS;
using Rena.Mathematics;

namespace Ink.Server.Entities.Components;

public readonly record struct EntitySyncedComponent(Vec3<double> LastSyncedPosition = default, Vec3<double> LastSyncedVelocity = default, Vec2<float> LastSyncedRotation = default, float LastSyncedHeadYaw = default) : IComponent
{
    public readonly Vec3<double> LastSyncedPosition = LastSyncedPosition;
    public readonly Vec3<double> LastSyncedVelocity = LastSyncedVelocity;
    public readonly Vec2<float> LastSyncedRotation = LastSyncedRotation;
    public readonly float LastSyncedHeadYaw = LastSyncedHeadYaw;
}
