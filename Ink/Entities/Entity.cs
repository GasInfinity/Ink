using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Command;
using Ink.Math;
using Ink.Text;
using Ink.Util;
using Ink.Util.Extensions;
using Ink.World;
using Rena.Mathematics;
using Rena.Native.Buffers.Extensions;
using Rena.Native.Extensions;
using System.Buffers;
using System.Collections.Immutable;

namespace Ink.Entities;

public abstract class Entity : ITickable, IMetaProvider, IAsyncCommandSender
{
    private static int currentEntityId = -1;

    public static int NextEntityId
        => Interlocked.Increment(ref currentEntityId);

    protected BaseWorld? world;
    protected uint ticks;
    protected uint ticksFloating;

    protected bool onGround;
    protected DirtyValue<EntityMask> mask = new();
    protected DirtyValue<int> airTicks = new(300);
    protected DirtyValue<TextPart?> customName = new();
    protected DirtyValue<bool> isCustomNameVisible = new();
    protected DirtyValue<bool> isSilent = new();
    protected DirtyValue<bool> hasNoGravity = new();
    protected DirtyValue<EntityPose> pose = new();
    protected DirtyValue<int> ticksFrozen = new();

    protected Vec3<double> position;
    protected Vec2<float> rotation;
    protected Vec3<double> velocity;

    protected Vec3<double> lastPosition;
    protected Vec2<float> lastRotation;
    protected Vec3<double> lastVelocity;

    public readonly int EntityId;
    public readonly Uuid Uuid;
    public readonly EntityDefinition Definition;
    public readonly IEntityTracker Tracker; // TODO: This can't be here if we want a general purpose library...
    public readonly ImmutableArray<int> CachedEntityId;

    public bool NoClip;

    public Vec3<double> Position { get => this.position; set => this.position = value; }

    public Vec2<float> Rotation { get => this.rotation; set => this.rotation = value; }

    public Vec3<double> Velocity { get => this.velocity; set => this.velocity = value; }

    public Vec3<double> LastPosition
        => this.lastPosition;

    public Vec2<float> LastRotation
        => this.lastRotation;

    public Vec3<double> LastVelocity
        => this.lastVelocity;

    public bool IsSneaking
    {
        get => this.pose.Value == EntityPose.Sneaking;
        set
        {
            if (value)
            {
                this.mask.Value |= EntityMask.Crouching;
                this.pose.Value = EntityPose.Sneaking;
            }
            else
            {
                this.mask.Value &= ~EntityMask.Crouching;
                this.pose.Value = EntityPose.Standing;
            }
        }
    }

    public bool IsSprinting
    {
        get => this.mask.Value.HasFlag(EntityMask.Sprinting);
        set
        {
            if (value)
                this.mask.Value |= EntityMask.Sprinting;
            else
                this.mask.Value &= ~EntityMask.Sprinting;
        }
    }

    public bool IsGlowing
    {
        get => this.mask.Value.HasFlag(EntityMask.HasGlowingEffect);
        set
        {
            if (value)
                this.mask.Value |= EntityMask.HasGlowingEffect;
            else
                this.mask.Value &= ~EntityMask.HasGlowingEffect;
        }
    }

    public bool NoGravity
    {
        get => this.hasNoGravity.Value;
        set => this.hasNoGravity.Value = value;
    }

    public bool OnGround
        => onGround;

    public BaseWorld? World
        => this.world;

    public virtual bool HasDirtyMetadata
        => this.mask.IsDirty
        || this.airTicks.IsDirty
        || this.customName.IsDirty
        || this.isCustomNameVisible.IsDirty
        || this.isSilent.IsDirty
        || this.hasNoGravity.IsDirty
        || this.pose.IsDirty
        || this.ticksFrozen.IsDirty;

    public virtual Aabb Box
        => Aabb.FromCenterMinCenter(position.X, position.Y, position.Z, Definition.CollisionBox);

    public virtual double EyeHeight
        => Definition.EyeHeight;

    public bool ObstructsBlockPlacements
        => Definition.ObstructsBlockPlacements;

    public bool HasVelocity
        => this.onGround ? (double.Abs(velocity.X) != 0 || double.Abs(velocity.Z) != 0 || velocity.Y > 0) : velocity != default;

    protected Entity(int entityId, Uuid uuid, EntityDefinition definition, IEntityTrackerFactory trackerFactory)
    {
        EntityId = entityId;
        CachedEntityId = [entityId];
        Uuid = uuid;
        Definition = definition;
        Tracker = trackerFactory.Create(this);
    }

    protected Entity(Uuid uuid, EntityDefinition definition, IEntityTrackerFactory trackerFactory) : this(NextEntityId, uuid, definition, trackerFactory)
    {
    }

    protected Entity(EntityDefinition definition, IEntityTrackerFactory trackerFactory) : this((Uuid)Guid.NewGuid(), definition, trackerFactory)
    {
    }

    public void Tick()
    {
        TickLogic();
        TickPhysics();
        // Tracker.Tick();

        lastPosition = position;
        lastVelocity = velocity;
        ++this.ticks;
    }

    protected virtual void TickLogic()
    {
    }

    protected virtual void TickPhysics()
    {
        const double SlipperinessConstant = 0.91;

        this.velocity = Move(this.velocity);
        double gravity = Definition.GravityAcceleration;
        double yDrag = Definition.VerticalDrag;
        bool dragBefore = Definition.DragBeforeAcceleration;

        double yVelocity = dragBefore ? (this.velocity.Y * yDrag) - gravity : (this.velocity.Y - gravity) * yDrag;

        Vec2<double> horzVelocity = this.velocity.Xz;

        if(onGround)
        {
            BlockPosition slipperyBlock = (BlockPosition)(this.Position - new Vec3<double>(0, 0.5, 0));
            BlockState state = world!.GetBlockState(slipperyBlock);
            Block? block = state.GetBlock(world.BlockRegistry);

            if(block != null)
            {
                float slipperiness = block.GetSlipperiness(state, world, slipperyBlock);
                horzVelocity *= SlipperinessConstant * slipperiness;
            }
        }

        this.velocity = new(horzVelocity.X, yVelocity, horzVelocity.Y);
    }

    public Vec3<double> Move(in Vec3<double> delta)
    {
        Vec3<double> adjustedDelta = NoClip ? delta : Collide(delta);
        this.position += adjustedDelta;
        return adjustedDelta;
    }

    private Vec3<double> Collide(in Vec3<double> delta)
    {
        Aabb currentBox = Box;
        Aabb adjustedBox = currentBox.Stretch(delta);
        ImmutableArray<(BlockPosition, Collider)> blockCollisions = new BlockCollisionEnumerable(World!, adjustedBox).ToImmutableArray();

        bool hasMovementX = !delta.X.AlmostEqual(0);

        double adjustedX = !hasMovementX ? 0 : delta.X;
        double adjustedY = Collider.TotalAdjustedMovementY(blockCollisions, adjustedBox, delta.Y);

        bool onGround = delta.Y < 0 && adjustedY.AlmostEqual(0);

        if(onGround)
            this.onGround = onGround;

        bool hasHigherZ = delta.Z > delta.X;
        bool shouldProcessFirstZ = hasHigherZ && hasMovementX;

        if (!shouldProcessFirstZ)
        {
            adjustedBox = currentBox.Stretch(new(delta.X, adjustedY, delta.Z));
            adjustedX = Collider.TotalAdjustedMovementX(blockCollisions, adjustedBox, delta.X);
        }

        adjustedBox = currentBox.Stretch(new(adjustedX, adjustedY, delta.Z));
        double adjustedZ = Collider.TotalAdjustedMovementZ(blockCollisions, adjustedBox, delta.Z);

        if (shouldProcessFirstZ)
        {
            adjustedBox = currentBox.Stretch(new(adjustedX, adjustedY, adjustedZ));
            adjustedX = Collider.TotalAdjustedMovementX(blockCollisions, adjustedBox, adjustedX);
        }

        bool xCollision = adjustedX.AlmostEqual(0) && !double.Abs(delta.X).AlmostEqual(0);
        bool zCollision = adjustedZ.AlmostEqual(0) && !double.Abs(delta.Z).AlmostEqual(0);
        bool horzCollision = xCollision || zCollision;
        return new(adjustedX, adjustedY, adjustedZ);
    }

    public virtual void SetWorld(BaseWorld newWorld, in Vec3<double> spawnPosition, Vec2<float> spawnRotation)
    {
        BaseWorld? lastWorld = this.world;

        if (lastWorld == newWorld)
        {
            Teleport(spawnPosition, spawnRotation);
            return;
        }

        this.world = newWorld;

        BlockPosition spawnBlockPos = (BlockPosition)spawnPosition;
        lastPosition = position = spawnPosition;
    }

    public virtual void Teleport(in Vec3<double> position, Vec2<float> rotation) // TODO: Async
    {
        this.position = position;
        this.rotation = rotation;
    }

    public virtual void WriteDirtyMetaAndClear(IBufferWriter<byte> writer)
    {
        if (this.mask.IsDirty)
        {
            writer.WriteMetaHeader(0, EntityMetaType.Byte);
            writer.WriteRaw((byte)this.mask.Value);
            this.mask.ClearDirty();
        }

        if (this.airTicks.IsDirty)
        {
            writer.WriteMetaHeader(1, EntityMetaType.VarInt);
            writer.WriteVarInteger(this.airTicks.Value);
            this.airTicks.ClearDirty();
        }

        if (this.customName.IsDirty)
        {
            writer.WriteMetaHeader(2, EntityMetaType.OptChat);

            TextPart? customName = this.customName.Value;

            if (writer.TryWriteOptional(customName != null))
                writer.WriteJsonJUtf8<TextPart>(customName!.Value, InkJsonContext.Default.TextPart);
            this.customName.ClearDirty();
        }

        if (this.isCustomNameVisible.IsDirty)
        {
            writer.WriteMetaHeader(3, EntityMetaType.Boolean);
            writer.WriteRaw(this.isCustomNameVisible.Value.AsByte());
            this.isCustomNameVisible.ClearDirty();
        }

        if (this.isSilent.IsDirty)
        {
            writer.WriteMetaHeader(4, EntityMetaType.Boolean);
            writer.WriteRaw(this.isSilent.Value.AsByte());
            this.isSilent.ClearDirty();
        }

        if (this.hasNoGravity.IsDirty)
        {
            writer.WriteMetaHeader(5, EntityMetaType.Boolean);
            writer.WriteRaw(this.hasNoGravity.Value.AsByte());
            this.hasNoGravity.ClearDirty();
        }

        if (this.pose.IsDirty)
        {
            writer.WriteMetaHeader(6, EntityMetaType.Pose);
            writer.WriteVarInteger((int)this.pose.Value);
            this.pose.ClearDirty();
        }

        if (this.ticksFrozen.IsDirty)
        {
            writer.WriteMetaHeader(7, EntityMetaType.VarInt);
            writer.WriteVarInteger((int)this.ticksFrozen.Value);
            this.ticksFrozen.ClearDirty();
        }
    }

    public virtual void WriteMeta(IBufferWriter<byte> writer) // TODO: Check if it is a default value?
    {
        writer.WriteMetaHeader(0, EntityMetaType.Byte);
        writer.WriteRaw((byte)this.mask.Value);

        writer.WriteMetaHeader(1, EntityMetaType.VarInt);
        writer.WriteVarInteger(this.airTicks.Value);

        writer.WriteMetaHeader(2, EntityMetaType.OptChat);
        TextPart? customName = this.customName.Value;

        if (writer.TryWriteOptional(customName != null))
            writer.WriteJsonJUtf8(customName!.Value, InkJsonContext.Default.TextPart);

        writer.WriteMetaHeader(3, EntityMetaType.Boolean);
        writer.WriteRaw(this.isCustomNameVisible.Value.AsByte());

        writer.WriteMetaHeader(4, EntityMetaType.Boolean);
        writer.WriteRaw(this.isSilent.Value.AsByte());

        writer.WriteMetaHeader(5, EntityMetaType.Boolean);
        writer.WriteRaw(this.hasNoGravity.Value.AsByte());

        writer.WriteMetaHeader(6, EntityMetaType.Pose);
        writer.WriteVarInteger((int)this.pose.Value);

        writer.WriteMetaHeader(7, EntityMetaType.VarInt);
        writer.WriteVarInteger((int)this.ticksFrozen.Value);
    }

    /// <summary>
    /// Removes an entity from the world/region
    /// </summary>
    public virtual void Remove()
    {
        Tracker.Remove();
        this.world?.RemoveEntity(this);
        this.world = null;
    }
}
