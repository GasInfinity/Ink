using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Math;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;

namespace Ink.Server.Worlds.Systems;

public sealed class SyncChunkCacheCenterSystem : QuerySystem<EntityRemotePlayerComponent, EntityTransformComponent, EntityLastTransformComponent>
{
    protected override void OnUpdate()
    {
        Query.Each(new SendCenterEach());
    }

    private readonly struct SendCenterEach() : IEach<EntityRemotePlayerComponent, EntityTransformComponent, EntityLastTransformComponent>
    {
        public void Execute(ref EntityRemotePlayerComponent remotePlayer, ref EntityTransformComponent t, ref EntityLastTransformComponent l)
        {
            BlockPosition blockPosition = (BlockPosition)t.Position;
            BlockPosition lastBlockPosition = (BlockPosition)l.Position;

            SectionPosition sectionPosition = blockPosition.ToSectionPosition();
            SectionPosition lastSectionPosition = lastBlockPosition.ToSectionPosition();

            if(sectionPosition != lastSectionPosition)
                remotePlayer.Connection.Send(new ClientboundSetChunkCacheCenter(sectionPosition.X, sectionPosition.Z));
        }
    }
}
