using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Ink.Server.Worlds.Systems;

public sealed class DeletionSystem : QuerySystem
{
    public DeletionSystem() => Filter.WithDisabled().AllTags(Tags.Get<Disabled>());

    protected override void OnUpdate()
    {
        foreach(Entity entity in Query.Entities)
        {
            CommandBuffer.DeleteEntity(entity.Id);
        }
    }
}
