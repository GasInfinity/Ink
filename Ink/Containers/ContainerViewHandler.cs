using Ink.Items;
using Ink.Net;
using Ink.Net.Packets.Play;

namespace Ink.Containers;

public abstract class InventoryViewHandler(byte windowId)
{
    private static int SharedWindowIdCounter = 0;
    private static byte NextSharedWindowId
        => (byte)(SharedWindowIdCounter = Interlocked.Increment(ref SharedWindowIdCounter));
    public readonly byte WindowId = windowId;
    public ItemStack CursorStack;

    protected InventoryViewHandler() : this(NextSharedWindowId)
    {
    }

    public void UpdateInventory(IConnection connection)
    {
        
    }

    public virtual bool TryHandleCreativeSetSlot(in ServerboundSetCreativeModeSlot setCreativeModeSlot)
        => false;

    public static int ToPlayerSlot(int networkInventorySlot)
    {
        if(networkInventorySlot < 4)
            return networkInventorySlot + PlayerContainer.ArmorStart; // ARMOR

        if(networkInventorySlot < 31)
            return networkInventorySlot + 5; // MAIN INVENTORY

        if(networkInventorySlot >= 31 && networkInventorySlot < 31 + 9)
            return networkInventorySlot - 31; // HOTBAR

        return networkInventorySlot; // OFFHAND
    }
}
