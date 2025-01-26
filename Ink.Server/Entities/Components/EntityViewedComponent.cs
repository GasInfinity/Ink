using Friflo.Engine.ECS;

namespace Ink.Server.Entities.Components;

public record struct EntityViewedComponent(List<int> LastViewers, List<int> Viewers) : IComponent
{
    public List<int> LastViewers = LastViewers;
    public List<int> Viewers = Viewers; 
}
