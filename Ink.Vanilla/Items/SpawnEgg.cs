using Ink.Entities;
using Ink.Items;
using Ink.Math;
using Ink.Registries;
using Ink.Util;
using Ink.World;

namespace Ink.Vanilla.Items;

public sealed class SpawnEggItem<T> : Item
    where T : Entity, IEntityFactory<T>
{
    public override Identifier Location { get; }

    public SpawnEggItem(Identifier iden)
        => Location = iden;

    public override ActionResult<ItemStack> UseOnBlock(ItemStack stack, PlayerEntity player, BaseWorld world, BlockPosition location, BlockFace face, float cursorX, float cursorY, float cursorZ, bool insideBlock)
    {
        T entity = world.SpawnEntity<T>(location.Relative(face));

        if(player.IsSneaking)
            entity.NoClip = true;

        return base.UseOnBlock(stack, player, world, location, face, cursorX, cursorY, cursorZ, insideBlock);
    }
}
