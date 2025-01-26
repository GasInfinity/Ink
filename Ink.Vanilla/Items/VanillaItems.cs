using Ink.Entities;
using Ink.Items;
using Ink.Registries;
using Ink.Vanilla.Blocks;

namespace Ink.Vanilla.Items;

public static class VanillaItems
{
    public static void RegisterAll(FrozenRegistryBuilder<Item> itemRegistry)
    {
        // itemRegistry.Register(Data.Item.Air, Air);
        // itemRegistry.Register(Data.Item.Stone, Stone);
        // itemRegistry.Register(Data.Item.OakPlanks, OakPlanks);
        // itemRegistry.Register(Data.Item.OakLog, OakLog);
        // itemRegistry.Register(Data.Item.OakLeaves, OakLeaves);
        // itemRegistry.Register(Data.Item.OakDoor, OakDoor);
        // itemRegistry.Register(Data.Item.Furnace, Furnace);
        // itemRegistry.Register(Data.Item.Ice, Ice);
        // itemRegistry.Register(Data.Item.BlueIce, BlueIce);
        //
        // itemRegistry.Register(Data.Item.CowSpawnEgg, TestSpawnEgg);
    }

    public static readonly BlockItem Air = new(VanillaBlocks.Air);
    public static readonly BlockItem Stone = new(VanillaBlocks.Stone);
    public static readonly BlockItem OakPlanks = new(VanillaBlocks.OakPlanks);
    public static readonly BlockItem OakLog = new(VanillaBlocks.OakLog);
    public static readonly BlockItem OakLeaves = new(VanillaBlocks.OakLeaves);
    public static readonly BlockItem OakDoor = new(VanillaBlocks.OakDoor);

    public static readonly BlockItem Furnace = new(VanillaBlocks.Furnace);

    public static readonly BlockItem Ice = new(VanillaBlocks.Ice);
    public static readonly BlockItem BlueIce = new(VanillaBlocks.BlueIce);

    // public static readonly SpawnEggItem<TestEntity> TestSpawnEgg = new(new("minecraft", "cow_spawn_egg"));
}
