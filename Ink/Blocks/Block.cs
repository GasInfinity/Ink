using Ink.Blocks.State;
using Ink.Entities;
using Ink.Items;
using Ink.Math;
using Ink.Registries;
using Ink.Util;
using Ink.World;

namespace Ink.Blocks;

public class Block : IHasLocation
{
    public readonly BlockStateRoot Root;
    public readonly Settings BlockSettings;

    public Identifier Location
        => Root.Identifier;

    public ref readonly BlockStateChild DefaultState
        => ref Root.Default;

    public Block(BlockStateRoot stateRoot, Settings settings)
        => (Root, BlockSettings) = (stateRoot, settings);

    public virtual BlockStateChild ComputePlacementState(in ItemPlacementContext context)
        => DefaultState;

    public virtual bool CanPlaceAt(BaseWorld world, BlockPosition position)
        => world.IsValid(position);

    public virtual bool CanReplace(in BlockStateChild state, in ItemPlacementContext context)
        => BlockSettings.IsReplaceable;

    public virtual void OnStateReplaced(in BlockStateChild state, BaseWorld world, BlockPosition position, in BlockStateChild newState, bool moved)
    {
    }

    public virtual void OnBlockAdded(in BlockStateChild state, BaseWorld world, BlockPosition position, in BlockStateChild oldState, bool notify)
    {
    }

    public virtual void OnPlaced(BaseWorld world, BlockPosition position, BlockStateChild state, LivingEntity placer, ItemStack stack)
    {
    }

    public virtual void OnBreak(BaseWorld world, BlockPosition position, BlockStateChild state, PlayerEntity player)
    {
    }

    public virtual Collider GetCollider(in BlockStateChild state, BaseWorld world, BlockPosition position)
    {
        return BlockSettings.IsCollidable ? Collider.Cube : Collider.Empty;
    }

    public virtual float GetSlipperiness(in BlockStateChild state, BaseWorld world, BlockPosition position)
    {
        return BlockSettings.Slipperiness;
    }

    public virtual ActionResult OnUse(in BlockStateChild state, BaseWorld world, BlockPosition position, PlayerEntity player, Hand hand)
    {
        return ActionResult.Pass;
    }

    public override string ToString()
        => Root.Identifier.ToString();

    public readonly record struct Settings(bool IsCollidable = true, bool IsReplaceable = false, float Slipperiness = 0.6f)
    {
        public static readonly Settings Default = new();

        public readonly bool IsCollidable = IsCollidable;
        public readonly bool IsReplaceable = IsReplaceable;
        public readonly float Slipperiness = Slipperiness;
        public Settings() : this(true, false, 0.6f)
        {
        }

        public Settings Replaceable(bool replaceable)
            => new(IsCollidable, replaceable, Slipperiness);

        public Settings Collidable(bool collidable)
            => new(collidable, IsReplaceable, Slipperiness);
        
        public Settings SetSlipperiness(float slipperiness)
            => new(IsCollidable, IsReplaceable, slipperiness);
    }
}
