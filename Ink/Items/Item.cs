using Ink.Entities;
using Ink.Math;
using Ink.Registries;
using Ink.Util;
using Ink.Worlds;

namespace Ink.Items;

public abstract class Item : IHasLocation
{
    public abstract Identifier Location { get; }

    public virtual ActionResult<ItemStack> UseOnBlock(ItemStack stack, PlayerEntity player, World world, BlockPosition location, BlockFace face, float cursorX, float cursorY, float cursorZ, bool insideBlock)
    {
        return ActionResult<ItemStack>.Pass;
    }

    public virtual ActionResult<ItemStack> UseOnEntity(ItemStack stack, PlayerEntity player, LivingEntity entity, Hand hand)
    {
        return ActionResult<ItemStack>.Pass;
    }

    public virtual ActionResult<ItemStack> Use(ItemStack stack, World world, PlayerEntity player)
    {
        return ActionResult<ItemStack>.Pass;
    }
}
