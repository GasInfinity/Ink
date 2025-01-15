using Ink.Blocks.State;
using Ink.Entities;
using Ink.Items;
using Ink.Math;
using Ink.Util;
using Ink.World;

namespace Ink.Blocks;

public class DoorBlock : Block
{
    public DoorBlock(BlockStateRoot stateRoot, Settings settings) : base(stateRoot, settings)
    {
    }

    public override BlockStateChild ComputePlacementState(in ItemPlacementContext context)
    {
        Direction hDirection = context.HorizontalPlayerDirection;

        return DefaultState.WithProperty(Properties.Half, TallBlockPart.Lower)
                           .WithProperty(Properties.Facing, hDirection.Horizontal)
                           .WithProperty(Properties.Hinge, ((hDirection.HorizontalAxis == Direction.HorizontalAxes.X) ? (context.CursorZ > 0.5f) : (context.CursorX < 0.5f)) ? DoorHinge.Right : DoorHinge.Left);
    }

    public override bool CanPlaceAt(BaseWorld world, BlockPosition position)
    {
        BlockPosition upper = position.Relative(y: 1);
        BlockPosition lower = position.Relative(y: -1);

        if (!world.IsInBuildLimit(upper) || !world.IsInBuildLimit(lower))
            return false;

        BlockStateChild upperState = world.GetBlockState(upper);
        BlockStateChild lowerState = world.GetBlockState(lower);

        // TODO Check for air in some other way, there's cave_air and void_air...
        if (upperState.Id != 0 || lowerState.Id == 0)
            return false;

        return base.CanPlaceAt(world, position);
    }

    public override void OnPlaced(BaseWorld world, BlockPosition position, BlockStateChild state, LivingEntity placer, ItemStack stack)
    {
        world.SetBlockState(position.Relative(y: 1), state.WithProperty(Properties.Half, TallBlockPart.Upper));
    }

    public override void OnBreak(BaseWorld world, BlockPosition position, BlockStateChild state, PlayerEntity player)
    {
        TallBlockPart part = (TallBlockPart)state.GetProperty(Properties.Half);

        if (part == TallBlockPart.Upper)
            world.SetBlockState(position.Relative(y: -1), default);
        else
            world.SetBlockState(position.Relative(y: 1), default);

        base.OnBreak(world, position, state, player);
    }

    public override ActionResult OnUse(in BlockStateChild state, BaseWorld world, BlockPosition position, PlayerEntity player, Hand hand)
    {
        TallBlockPart part = (TallBlockPart)state.GetProperty(Properties.Half);
        bool open = state.GetProperty(Properties.Open) != 0;
        bool inverseState = !open;

        world.SetBlockState(position, state.WithProperty(Properties.Open, inverseState));

        if(part == TallBlockPart.Upper)
            world.SetBlockState(position.Relative(y: -1), state.WithProperty(Properties.Half, TallBlockPart.Lower).WithProperty(Properties.Open, inverseState));
        else
            world.SetBlockState(position.Relative(y: 1), state.WithProperty(Properties.Half, TallBlockPart.Upper).WithProperty(Properties.Open, inverseState));

        return ActionResult.Success;
    }
}
