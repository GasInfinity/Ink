using Friflo.Engine.ECS;

namespace Ink.Entities.Components;

public record struct EntityHealthComponent(float Health) : IComponent
{
    public float Health = Health;
}
