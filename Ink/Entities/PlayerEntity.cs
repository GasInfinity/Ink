using Ink.Auth;
using Ink.Util;
using Ink.Util.Extensions;
using Ink.World;
using Rena.Native.Buffers.Extensions;
using System.Buffers;

namespace Ink.Entities;

public abstract class PlayerEntity : LivingEntity
{
    public readonly GameProfile Profile;

    protected DirtyValue<PlayerSkinPart> displayedSkin;
    protected DirtyValue<PlayerMainHand> mainHand = new(PlayerMainHand.Right);

    public GameMode CurrentGameMode { get; protected set; }

    public PlayerMainHand MainHand
    {
        get => this.mainHand.Value;
        set => this.mainHand.Value = value;
    }

    public override bool HasDirtyMetadata
        => base.HasDirtyMetadata
        || this.displayedSkin.IsDirty
        || this.mainHand.IsDirty;

    protected PlayerEntity(GameProfile profile, IEntityTrackerFactory trackerFactory)
        : base(profile.Id, EntityDefinition.Player, trackerFactory)
    {
        Profile = profile;
    }

    public override void WriteDirtyMetaAndClear(IBufferWriter<byte> writer)
    {
        base.WriteDirtyMetaAndClear(writer);

        if(this.displayedSkin.IsDirty)
        {
            writer.WriteMetaHeader(17, EntityMetaType.Byte);
            writer.WriteRaw((byte)this.displayedSkin.Value);
            this.displayedSkin.ClearDirty();
        }

        if (this.mainHand.IsDirty)
        {
            writer.WriteMetaHeader(18, EntityMetaType.Byte);
            writer.WriteRaw((byte)this.mainHand.Value);
            this.mainHand.ClearDirty();
        }
    }

    public override void WriteMeta(IBufferWriter<byte> writer)
    {
        base.WriteMeta(writer);

        writer.WriteMetaHeader(17, EntityMetaType.Byte);
        writer.WriteRaw((byte)this.displayedSkin.Value);

        writer.WriteMetaHeader(18, EntityMetaType.Byte);
        writer.WriteRaw((byte)this.mainHand.Value);
    }

    public struct FoodHandler
    {
        private int lastFoodLevel;
        public int FoodLevel;
        public float Saturation;
        public float Exhaustion;

        public readonly int LastFoodLevel
            => lastFoodLevel;

        public void Tick(PlayerEntity player)
        {
            lastFoodLevel = FoodLevel;
        }
    }
}
