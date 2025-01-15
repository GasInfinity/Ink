using System.Collections.Frozen;

namespace Ink.Registries;

public static class RegistriesInfo 
{
    public static FrozenSet<RegistryKey> SyncedRegistries = [
        RegistryKeys.ChatType,
        RegistryKeys.DamageType,
        RegistryKeys.DimensionType,
        RegistryKeys.PaintingVariant,
        RegistryKeys.WolfVariant,
        RegistryKeys.Worldgen.Biome
    ];

    public static FrozenSet<RegistryKey> DynamicRegistries = SyncedRegistries.Concat([
    ]).ToFrozenSet();
}
