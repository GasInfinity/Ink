using Ink.Items;
using Ink.Math;
using Ink.Util;
using Ink.Util.Extensions;
using System.Buffers;

namespace Ink.Entities;

public class ItemEntity : Entity, IEntityFactory<ItemEntity>
{
    protected DirtyValue<ItemStack> slot;
    
    public ItemStack Slot
    {
        get => this.slot.Value;
        set => this.slot.Value = value;
    }

    public override bool HasDirtyMetadata
        => base.HasDirtyMetadata
        || slot.IsDirty;

    public ItemEntity(IEntityTrackerFactory trackerFactory) : base(EntityDefinition.Item, trackerFactory)
    {
    }

    public ItemEntity(Uuid uuid, IEntityTrackerFactory trackerFactory) : base(uuid, EntityDefinition.Item, trackerFactory)
    {
    }

    public ItemEntity(int entityId, Uuid uuid, IEntityTrackerFactory trackerFactory) : base(entityId, uuid, EntityDefinition.Item, trackerFactory)
    {
    }

    protected override void TickLogic()
    {
        if(Position.Y < 0)
        {
            Remove();
            return;
        }

        if(Slot.IsEmpty)
        {
            Remove();
            return;
        }

        base.Tick();
        //TryMerge();
    }

    public override void WriteDirtyMetaAndClear(IBufferWriter<byte> writer)
    {
        base.WriteDirtyMetaAndClear(writer);

        if (this.slot.IsDirty)
        {
            writer.WriteMetaHeader(8, EntityMetaType.Slot);
            this.slot.Value.Write(writer);
            this.slot.ClearDirty();
        }
    }

    public override void WriteMeta(IBufferWriter<byte> writer)
    {
        base.WriteMeta(writer);

        writer.WriteMetaHeader(8, EntityMetaType.Slot);
        this.slot.Value.Write(writer);
    }

    private void TryMerge()
    {
        ItemStack stack = Slot;
        
        if(stack.Count >= 64)
            return;
        
        foreach(var entity in World!.NearbyEntities((BlockPosition)position))
        {
            if(entity is not ItemEntity item || item == this)
                continue;
            
            ItemStack other = item.Slot;

            if(!stack.TryMerge(ref other, out ItemStack newStack))
                continue;
            
            Slot = newStack;
            item.Slot = other;
        }
    }


    public static ItemEntity Create(IEntityTrackerFactory trackerFactory)
        => new(trackerFactory);

    public static ItemEntity Create(Uuid uuid, IEntityTrackerFactory trackerFactory)
        => new(uuid, trackerFactory);

    public static ItemEntity Create(int entityId, Uuid uuid, IEntityTrackerFactory trackerFactory)
        => new(entityId, uuid, trackerFactory);
    
}
