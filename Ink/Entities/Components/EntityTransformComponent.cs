using Friflo.Engine.ECS;
using Rena.Mathematics;

namespace Ink.Entities.Components;

public record struct EntityTransformComponent(Vec3<double> Position = default, Vec3<double> Velocity = default, Vec2<float> Rotation = default, float HeadYaw = default) : IComponent
{
    public Vec3<double> Position = Position;
    public Vec3<double> Velocity = Velocity;
    public Vec2<float> Rotation = Rotation;
    public float HeadYaw = HeadYaw;
}

public readonly record struct EntityLastTransformComponent(Vec3<double> Position = default, Vec3<double> Velocity = default, Vec2<float> Rotation = default, float HeadYaw = default) : IComponent
{
    public readonly Vec3<double> Position = Position;
    public readonly Vec3<double> Velocity = Velocity;
    public readonly Vec2<float> Rotation = Rotation;
    public readonly float HeadYaw = HeadYaw;
}
