using Friflo.Engine.ECS;

namespace Ink.Entities.Components;

public record struct EntityPhysicsComponent(bool NoClip, bool NoGravity, int TicksFloating) : IComponent
{
    public bool NoClip = NoClip;
    public bool NoGravity = NoGravity;
    public int TicksFloating = TicksFloating;
}
