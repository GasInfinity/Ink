using Ink.Containers;
using Ink.Items;
using Ink.Math;
using Ink.Util;
using Ink.Util.Extensions;
using Rena.Mathematics;
using Rena.Native.Buffers.Extensions;
using System.Buffers;

namespace Ink.Entities;

public abstract class LivingEntity : Entity
{
    public const int SwapHandsEntityStatus = 55;

    private readonly ItemStack[] lastSynchedEquipment = new ItemStack[(int)EquipmentSlot.Slots];
    protected DirtyValue<float> health = new(1f);
    private float currentHeadYaw;

    public override bool HasDirtyMetadata
        => base.HasDirtyMetadata
        || this.health.IsDirty;

    public ReadOnlySpan<ItemStack> LastSynchedEquipment
        => this.lastSynchedEquipment;

    public bool HasSynchedEquipment
    {
        get
        {
            foreach(ItemStack synched in this.lastSynchedEquipment)
            {
                if(!synched.IsEmpty)
                    return true;
            }

            return false;
        }
    }

    public float Health // TODO: Attributes
    {
        get => this.health.Value;
        set
        {
            this.health.Value = value;
        }
    }

    public float CurrentHeadYaw { get => this.currentHeadYaw; set => this.currentHeadYaw = value; }

    public Direction HorizontalDirection
        => Direction.FromRotation(CurrentHeadYaw);

    public Direction Direction
        => Direction.FromRotations(CurrentHeadYaw, this.rotation.Y);

    public Vec3<double> HeadForwardVector
    {
        get
        {
            double y = double.Sin(Radians<double>.FromDegrees(-this.rotation.Y).Value);
            (double xMinus, double zPlus) = double.SinCos(Radians<double>.FromDegrees(CurrentHeadYaw).Value);
            double oneMinusY = 1 - double.Abs(y);
            return new(-xMinus * oneMinusY, y, zPlus * oneMinusY);
        }
    }

    public virtual ItemStack this[EquipmentSlot slot] { get => default; set {} }

    protected LivingEntity(int entityId, Uuid uuid, EntityDefinition definition, IEntityTrackerFactory trackerFactory) : base(entityId, uuid, definition, trackerFactory)
    {
    }

    protected LivingEntity(Uuid uuid, EntityDefinition definition, IEntityTrackerFactory trackerFactory) : base(uuid, definition, trackerFactory)
    {
    }

    protected LivingEntity(EntityDefinition definition, IEntityTrackerFactory trackerFactory) : base(definition, trackerFactory)
    {
    }

    protected override void TickLogic()
    {
        base.Tick();

        TrySendDirtyEquipment();
    }

    public virtual void Swing(Hand hand = Hand.Main)
        // => Tracker.Send(new CEntityAnimationPacket(EntityId, hand == Hand.Main ? CEntityAnimationPacket.Animations.SwingMain : CEntityAnimationPacket.Animations.SwingOff));
    {}

    private void TrySendDirtyEquipment()
    {
        ItemStack main = this[EquipmentSlot.MainHand];
        ItemStack off = this[EquipmentSlot.OffHand];
        ItemStack lastMain = this.lastSynchedEquipment[(int)EquipmentSlot.MainHand];
        ItemStack lastOff = this.lastSynchedEquipment[(int)EquipmentSlot.OffHand];

        int dirtyEquipmentLength = 0;
        Span<(EquipmentSlot, ItemStack)> dirtyEquipmentBuffer = stackalloc (EquipmentSlot, ItemStack)[(int)EquipmentSlot.Slots];

        bool changedMain = main != lastMain;
        bool changedOff = off != lastOff;
        if(changedMain || changedOff)
        {
            if(!TrySendSwappedhands(main, off, lastMain, lastOff))
            {
                if(changedMain)
                {
                    dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.MainHand, main);
                    this.lastSynchedEquipment[(int)EquipmentSlot.MainHand] = main;
                }

                if(changedOff)
                {
                    dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.OffHand, off);
                    this.lastSynchedEquipment[(int)EquipmentSlot.OffHand] = off;
                }
            }
        }

        ItemStack helmet = this[EquipmentSlot.Helmet];
        if(helmet != this.lastSynchedEquipment[(int)EquipmentSlot.Helmet])
        {
            dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.Helmet, helmet);
            this.lastSynchedEquipment[(int)EquipmentSlot.Helmet] = helmet;
        }

        ItemStack chestplate = this[EquipmentSlot.Chestplate];
        if(chestplate != this.lastSynchedEquipment[(int)EquipmentSlot.Chestplate])
        {
            dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.Chestplate, chestplate);
            this.lastSynchedEquipment[(int)EquipmentSlot.Chestplate] = chestplate;
        }

        ItemStack leggings = this[EquipmentSlot.Leggings];
        if(leggings != this.lastSynchedEquipment[(int)EquipmentSlot.Leggings])
        {
            dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.Leggings, leggings);
            this.lastSynchedEquipment[(int)EquipmentSlot.Leggings] = leggings;
        }

        ItemStack boots = this[EquipmentSlot.Boots];
        if(boots != this.lastSynchedEquipment[(int)EquipmentSlot.Boots])
        {
            dirtyEquipmentBuffer[dirtyEquipmentLength++] = (EquipmentSlot.Boots, boots);
            this.lastSynchedEquipment[(int)EquipmentSlot.Boots] = boots;
        }

        if(dirtyEquipmentLength > 0)
        {
            // using CSetEquipmentPacket equipmentPacket = CSetEquipmentPacket.FromDirtyEquipment(EntityId, dirtyEquipmentBuffer, dirtyEquipmentLength);
            // Tracker.Send(equipmentPacket);
        }
    }

    private bool TrySendSwappedhands(ItemStack main, ItemStack off, ItemStack lastMain, ItemStack lastOff)
    {
        if(main != lastOff || off != lastMain)
            return false;

        // Tracker.Send(new CEntityEventPacket(EntityId, SwapHandsEntityStatus));
        this.lastSynchedEquipment[(int)EquipmentSlot.MainHand] = main;
        this.lastSynchedEquipment[(int)EquipmentSlot.OffHand] = off;
        return true;
    }

    public override void WriteDirtyMetaAndClear(IBufferWriter<byte> writer)
    {
        base.WriteDirtyMetaAndClear(writer);

        if(this.health.IsDirty)
        {
            writer.WriteMetaHeader(9, EntityMetaType.Float);
            writer.WriteSingle(this.health.Value, false);
            this.health.ClearDirty();
        }
    }

    public override void WriteMeta(IBufferWriter<byte> writer)
    {
        base.WriteMeta(writer);

        writer.WriteMetaHeader(9, EntityMetaType.Float);
        writer.WriteSingle(this.health.Value, false);
    }
}
