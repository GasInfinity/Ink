using Ink.Blocks.State;
using Ink.Entities;
using Ink.Items;
using Ink.Math;
using Ink.Util;
using Ink.Worlds;

namespace Ink.Blocks;

public class DoorBlock : Block
{
    public DoorBlock(BlockStateRoot stateRoot, Settings settings) : base(stateRoot, settings)
    {
    }

    public override BlockState ComputePlacementState(in ItemPlacementContext context)
    {
        Direction hDirection = context.HorizontalPlayerDirection;

        return DefaultState.WithProperty(PropertyNames.Half, TallBlockPart.Lower)
                           .WithProperty(PropertyNames.Facing, hDirection.Value)
                           .WithProperty(PropertyNames.Hinge, ((hDirection.Axis == Direction.Axes.X) ? (context.CursorZ > 0.5f) : (context.CursorX < 0.5f)) ? DoorBlockHinge.Right : DoorBlockHinge.Left);
    }

    public override bool CanPlaceAt(World world, BlockPosition position)
    {
        BlockPosition upper = position.Relative(y: 1);
        BlockPosition lower = position.Relative(y: -1);

        if (!world.IsInBuildLimit(upper) || !world.IsInBuildLimit(lower))
            return false;

        BlockState upperState = world.GetBlockState(upper);
        BlockState lowerState = world.GetBlockState(lower);

        // TODO Check for air in some other way, there's cave_air and void_air...
        if (upperState.Id != 0 || lowerState.Id == 0)
            return false;

        return base.CanPlaceAt(world, position);
    }

    public override void OnPlaced(World world, BlockPosition position, BlockState state, LivingEntity placer, ItemStack stack)
    {
        world.SetBlockState(position.Relative(y: 1), state.WithProperty(PropertyNames.Half, TallBlockPart.Upper));
    }

    public override void OnBreak(World world, BlockPosition position, BlockState state, PlayerEntity player)
    {
        TallBlockPart part = state.GetProperty<TallBlockPart>(PropertyNames.Half);

        if (part == TallBlockPart.Upper)
            world.SetBlockState(position.Relative(y: -1), default);
        else
            world.SetBlockState(position.Relative(y: 1), default);

        base.OnBreak(world, position, state, player);
    }

    public override ActionResult OnUse(in BlockState state, World world, BlockPosition position, PlayerEntity player, Hand hand)
    {
        TallBlockPart part = state.GetProperty<TallBlockPart>(PropertyNames.Half);
        bool open = state.GetProperty<bool>(PropertyNames.Open);
        bool inverseState = !open;

        world.SetBlockState(position, state.WithProperty(PropertyNames.Open, inverseState));

        if(part == TallBlockPart.Upper)
            world.SetBlockState(position.Relative(y: -1), state.WithProperty(PropertyNames.Half, TallBlockPart.Lower).WithProperty(PropertyNames.Open, inverseState));
        else
            world.SetBlockState(position.Relative(y: 1), state.WithProperty(PropertyNames.Half, TallBlockPart.Upper).WithProperty(PropertyNames.Open, inverseState));

        return ActionResult.Success;
    }
}
