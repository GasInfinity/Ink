using Ink.Entities;
using Ink.Items;
using Ink.Net.Packets.Play;

namespace Ink.Containers;

public sealed class PlayerContainerViewHandler(PlayerEntity Player, PlayerContainer inventory) : InventoryViewHandler(0)
{
    public PlayerEntity Player = Player;
    public PlayerContainer Inventory = inventory;

    public override bool TryHandleCreativeSetSlot(in ServerboundSetCreativeModeSlot setCreativeModeSlot)
    {
        if(setCreativeModeSlot.Slot < 5)
        {
            // CRAFTING
        }
        else
        {
            Inventory[ToPlayerSlot(setCreativeModeSlot.Slot - 5)] = setCreativeModeSlot.ClickedItem;
        }

        return true;
    }
}
