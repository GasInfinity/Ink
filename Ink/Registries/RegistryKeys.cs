using Ink.Blocks;
using Ink.Chat;
using Ink.Entities;
using Ink.Entities.Damage;
using Ink.Items;
using Ink.Worlds;
using Ink.Worlds.Biomes;

namespace Ink.Registries;

public static class RegistryKeys
{
    public static readonly Identifier Root = Identifier.Vanilla("root");
    public static readonly RegistryKey<IReadOnlyRegistry<Item>> Item = new(Root, Identifier.Vanilla("item"));
    public static readonly RegistryKey<IReadOnlyRegistry<Block>> Block = new(Root, Identifier.Vanilla("block"));
    public static readonly RegistryKey<IReadOnlyRegistry<DimensionType>> DimensionType = new(Root, Identifier.Vanilla("dimension_type"));
    public static readonly RegistryKey<IReadOnlyRegistry<ChatType>> ChatType = new(Root, Identifier.Vanilla("chat_type"));
    public static readonly RegistryKey<IReadOnlyRegistry<DamageType>> DamageType = new(Root, Identifier.Vanilla("damage_type"));
    public static readonly RegistryKey<IReadOnlyRegistry<PaintingVariant>> PaintingVariant = new(Root, Identifier.Vanilla("painting_variant"));
    public static readonly RegistryKey<IReadOnlyRegistry<WolfVariant>> WolfVariant = new(Root, Identifier.Vanilla("wolf_variant"));

    public static class Worldgen
    {
        public static readonly RegistryKey<IReadOnlyRegistry<BiomeType>> Biome = new(Root, Identifier.Vanilla("worldgen/biome"));
    }
}
