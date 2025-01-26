using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;

namespace Ink.Server.Worlds.Systems;

public sealed class TransformToLastSystem : QuerySystem<EntityTransformComponent, EntityLastTransformComponent>
{
    protected override void OnUpdate()
    {
        Query.Each(new UpdateEach());
    }

    private readonly struct UpdateEach() : IEach<EntityTransformComponent, EntityLastTransformComponent>
    {
        public void Execute(ref EntityTransformComponent t, ref EntityLastTransformComponent l)
            => l = Unsafe.BitCast<EntityTransformComponent, EntityLastTransformComponent>(t); 
    }
}
