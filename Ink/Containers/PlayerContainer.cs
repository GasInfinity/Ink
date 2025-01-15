using Ink.Items;

namespace Ink.Containers;

public sealed class PlayerContainer : Container
{
    const int HotbarSize = 9;
    const int MainSize = 36;
    const int ArmorSize = 4;
    const int OffHandSize = 1;
    const int TotalSize = HotbarSize + MainSize + ArmorSize + OffHandSize;
    const int OffHandSlot = 40;
    public const int ArmorStart = 36;
    const int ArmorEnd = ArmorStart + 4;

    private byte rawHeldSlot = 0;
    public int HeldSlot { get => rawHeldSlot; set => this.rawHeldSlot = byte.Clamp((byte)value, 0, HotbarSize - 1); }

    public ItemStack HeldStack { get => this[this.rawHeldSlot]; set => this[this.rawHeldSlot] = value; }
    public ItemStack OffHandStack { get => this[OffHandSlot]; set => this[OffHandSlot] = value; }
    public ItemStack HelmetStack { get => this[ArmorStart]; set => this[ArmorStart] = value; }
    public ItemStack ChestplateStack { get => this[ArmorStart + 1]; set => this[ArmorStart + 1] = value; }
    public ItemStack LeggingsStack { get => this[ArmorStart + 2]; set => this[ArmorStart + 2] = value; }
    public ItemStack BootsStack { get => this[ArmorStart + 3]; set => this[ArmorStart + 3] = value; }

    public ItemStack this[EquipmentSlot slot]
    {
        get => slot switch
        {
            EquipmentSlot.MainHand => HeldStack,
            EquipmentSlot.OffHand => OffHandStack,
            EquipmentSlot.Helmet => HelmetStack,
            EquipmentSlot.Chestplate => ChestplateStack,
            EquipmentSlot.Leggings => LeggingsStack,
            EquipmentSlot.Boots => BootsStack,
            _ => default,
        };
        set
        {
            switch(slot)
            {
                case EquipmentSlot.MainHand: HeldStack = value; break;
                case EquipmentSlot.OffHand: OffHandStack = value; break;
                case EquipmentSlot.Helmet: HelmetStack = value; break;
                case EquipmentSlot.Chestplate: ChestplateStack = value; break;
                case EquipmentSlot.Leggings: LeggingsStack = value; break;
                case EquipmentSlot.Boots: BootsStack = value; break;
            }
        }
    }

    public PlayerContainer() : base(TotalSize)
    {
    }
}
