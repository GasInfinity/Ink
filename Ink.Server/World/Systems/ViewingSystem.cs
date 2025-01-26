using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Math;
using Ink.Server.Entities.Components;

namespace Ink.Server.Worlds.Systems;

// TODO: This could be pararellized
public sealed class ViewingSystem : QuerySystem<EntityTransformComponent, EntityViewedComponent>
{
    private ArchetypeQuery<EntityChunkViewerComponent>? chunkViewersQuery;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        this.chunkViewersQuery = store.Query<EntityChunkViewerComponent>();
    }

    protected override void OnUpdate()
    {
        Query.EachEntity(new SyncEach(this.chunkViewersQuery!));
    }

    private struct SyncEach(ArchetypeQuery<EntityChunkViewerComponent> chunkViewersQuery) : IEachEntity<EntityTransformComponent, EntityViewedComponent>
    {
        public void Execute(ref EntityTransformComponent t, ref EntityViewedComponent viewed, int id)
        {
            (viewed.LastViewers, viewed.Viewers) = (viewed.Viewers, viewed.LastViewers); 
            viewed.Viewers.Clear();

            chunkViewersQuery.EachEntity(new InRangeEach(id, ((BlockPosition)t.Position).ToChunkPosition(), viewed.Viewers));
        }

        private struct InRangeEach(int viewedId, ChunkPosition position, List<int> viewers) : IEachEntity<EntityChunkViewerComponent>
        {
            public void Execute(ref EntityChunkViewerComponent viewer, int id)
            {
                if(id == viewedId || !viewer.Viewing.Contains(position))
                    return;

                viewers.Add(id);
            }
        }
    }
}
