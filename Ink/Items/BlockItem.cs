using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Entities;
using Ink.Items;
using Ink.Math;
using Ink.Registries;
using Ink.Util;
using Ink.World;

namespace Ink.Vanilla.Items;

public sealed class BlockItem : Item
{
    public readonly Block Block;

    public override Identifier Location
        => Block.Location;

    public BlockItem(Block block)
        => Block = block;

    public override ActionResult<ItemStack> UseOnBlock(ItemStack stack, PlayerEntity player, BaseWorld world, BlockPosition location, BlockFace face, float cursorX, float cursorY, float cursorZ, bool insideBlock)
    {
        ItemPlacementContext context = new(player, face, cursorX, cursorY, cursorZ);

        if (!TryPlace(context, player, world, location))
        {
            BlockPosition nextLocation = location.Relative(face);

            if (TryPlace(context, player, world, nextLocation))
                return ActionResult<ItemStack>.Success(stack); // TODO!

            return ActionResult<ItemStack>.Pass;
        }

        return ActionResult<ItemStack>.Success(stack); // TODO!
    }

    private bool TryPlace(in ItemPlacementContext context, PlayerEntity player, BaseWorld world, BlockPosition location)
    {
        BlockState lastState = world.GetBlockState(location);
        Block? lastStateBlock = lastState.GetBlock(world.BlockRegistry);

        if (!(lastStateBlock?.CanReplace(lastState, context) ?? true))
            return false;

        if (!Block.CanPlaceAt(world, location))
            return false;

        BlockState state = Block.ComputePlacementState(context);

        if (!world.CanPlaceAt(location, state, Block, out _))
            return false;

        world.SetBlockState(location, state);
        Block.OnPlaced(world, location, state, player, default);
        return true;
    }
}
