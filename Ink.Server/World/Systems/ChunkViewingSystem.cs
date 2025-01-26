using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Math;
using Ink.Server.Entities.Components;

namespace Ink.Server.Worlds.Systems;

public sealed class ChunkViewingSystem : QuerySystem<EntityTransformComponent, EntityLastTransformComponent, EntityChunkViewerComponent>
{
    protected override void OnUpdate()
    {
        Query.Each(new UpdateChunksEach());
    }

    private readonly struct UpdateChunksEach() : IEach<EntityTransformComponent, EntityLastTransformComponent, EntityChunkViewerComponent>
    {
        public void Execute(ref EntityTransformComponent t, ref EntityLastTransformComponent l, ref EntityChunkViewerComponent viewer)
        {
            ChunkPosition lastPosition = ((BlockPosition)l.Position).ToChunkPosition();
            ChunkPosition position = ((BlockPosition)t.Position).ToChunkPosition();

            viewer.New.Clear();
            viewer.Old.Clear();

            if(viewer.Viewing.Count > 0 && position == lastPosition)
                return;

            int view = viewer.Distance;
            int startX = position.X - view;
            int endX = position.X + view;
            int startZ = position.Z - view;
            int endZ = position.Z + view;

            (viewer.LastViewing, viewer.Viewing) = (viewer.Viewing, viewer.LastViewing);
            viewer.Viewing.Clear();
            
            for (int x = startX; x <= endX; ++x)
            {
                for (int z = startZ; z <= endZ; ++z)
                {
                    ChunkPosition current = new(x, z);

                    if(viewer.Viewing.Add(current)
                    && !viewer.LastViewing.Contains(current))
                    {
                        viewer.New.Add(current);
                    }
                }
            }

            foreach(ChunkPosition last in viewer.LastViewing)
            {
                if(!viewer.Viewing.Contains(last))
                {
                    viewer.Old.Add(last);
                }
            }
        }
    }
}
