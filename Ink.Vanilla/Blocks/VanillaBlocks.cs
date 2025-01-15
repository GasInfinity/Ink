using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Registries;

namespace Ink.Vanilla.Blocks;

public static class VanillaBlocks
{
    public static void RegisterAll(FrozenRegistryBuilder<Block> blockRegistry)
    {
        // Block id's changed
        // blockRegistry.Register(Data.Block.Air, Air);
        // blockRegistry.Register(Data.Block.Stone, Stone);
        // blockRegistry.Register(Data.Block.OakPlanks, OakPlanks);
        // blockRegistry.Register(Data.Block.OakLog, OakLog);
        // blockRegistry.Register(Data.Block.OakLeaves, OakLeaves);
        // blockRegistry.Register(Data.Block.OakDoor, OakDoor);
        // blockRegistry.Register(Data.Block.Furnace, Furnace);
        // blockRegistry.Register(Data.Block.Ice, Ice);
        // blockRegistry.Register(Data.Block.BlueIce, BlueIce);
    }

    public static readonly Block Air = new(BlockStates.Air.Root, Block.Settings.Default.Replaceable(true).Collidable(false));
    public static readonly Block Stone = new(BlockStates.Stone.Root, Block.Settings.Default);
    public static readonly Block OakPlanks = new(BlockStates.OakPlanks.Root, Block.Settings.Default);
    public static readonly AxisFacingBlock OakLog = new(BlockStates.OakLog.Root, Block.Settings.Default);
    public static readonly Block OakLeaves = new(BlockStates.OakLeaves.Root, Block.Settings.Default);
    public static readonly DoorBlock OakDoor = new(BlockStates.OakDoor.Root, Block.Settings.Default);

    public static readonly HorizontalFacingBlock Furnace = new(BlockStates.Furnace.Root, Block.Settings.Default);
    public static readonly Block Ice = new(BlockStates.Ice.Root, Block.Settings.Default.SetSlipperiness(0.98f));
    public static readonly Block BlueIce = new(BlockStates.BlueIce.Root, Block.Settings.Default.SetSlipperiness(0.989f));
}
