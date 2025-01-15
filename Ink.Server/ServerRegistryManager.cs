using Ink.Blocks;
using Ink.Chat;
using Ink.Entities;
using Ink.Entities.Damage;
using Ink.Event.Registry;
using Ink.Items;
using Ink.Net.Structures;
using Ink.Registries;
using Ink.World;
using Ink.World.Biomes;
using System.Collections.Immutable;

namespace Ink.Server;

public sealed class ServerRegistryManager
{
    public readonly ImmutableArray<VersionedIdentifier> KnownPacks = [VersionedIdentifier.Vanilla("core", "1.21.4")];

    private bool frozen;

    private FrozenRegistry<IReadOnlyRegistry> root = FrozenRegistry<IReadOnlyRegistry>.Empty;
    private FrozenRegistry<Item> item = FrozenRegistry<Item>.Empty;
    private FrozenRegistry<Block> block = FrozenRegistry<Block>.Empty;
    private FrozenRegistry<BiomeType> biome = FrozenRegistry<BiomeType>.Empty;
    private FrozenRegistry<DimensionType> dimensionType = FrozenRegistry<DimensionType>.Empty;
    private FrozenRegistry<ChatType> chatType = FrozenRegistry<ChatType>.Empty;
    private FrozenRegistry<DamageType> damageType = FrozenRegistry<DamageType>.Empty;
    private FrozenRegistry<PaintingVariant> paintingVariant = FrozenRegistry<PaintingVariant>.Empty;
    private FrozenRegistry<WolfVariant> wolfVariant = FrozenRegistry<WolfVariant>.Empty;

    public IValueRegistrationListener<Item>? ItemRegistryListener { get; set; }
    public IValueRegistrationListener<Block>? BlockRegistryListener { get; set; }
    public IValueRegistrationListener<BiomeType>? BiomeRegistryListener { get; set; }
    public IValueRegistrationListener<DimensionType>? DimensionRegistryListener { get; set; }
    public IValueRegistrationListener<ChatType>? ChatTypeRegistryListener { get; set; }
    public IValueRegistrationListener<DamageType>? DamageTypeRegistryListener { get; set; }
    public IValueRegistrationListener<PaintingVariant>? PaintingVariantRegistryListener { get; set; }
    public IValueRegistrationListener<WolfVariant>? WolfVariantRegistryListener { get; set; }
    
    public IReadOnlyRegistry<IReadOnlyRegistry> Registries
        => this.root;

    public FrozenRegistry<Item> Item
        => this.item;

    public FrozenRegistry<Block> Block
        => this.block;

    public FrozenRegistry<BiomeType> Biome
        => this.biome;

    public FrozenRegistry<DimensionType> DimensionType
        => this.dimensionType;

    public FrozenRegistry<ChatType> ChatType
        => this.chatType;

    public FrozenRegistry<DamageType> DamageType
        => this.damageType;

    public ServerRegistryManager()
    {
    }

    public void RegisterFreeze()
    {
        if (this.frozen) // Do nothing
            return;

        FrozenRegistryBuilder<IReadOnlyRegistry> registries = new();
        FrozenRegistryBuilder<Item> item = new();
        FrozenRegistryBuilder<Block> block = new();
        FrozenRegistryBuilder<DimensionType> dimensionType = new();
        FrozenRegistryBuilder<ChatType> chatType = new();
        FrozenRegistryBuilder<DamageType> damageType= new();
        FrozenRegistryBuilder<BiomeType> biome = new();
        FrozenRegistryBuilder<PaintingVariant> paintingVariant= new();
        FrozenRegistryBuilder<WolfVariant> wolfVariant = new();

        ItemRegistryListener?.OnRegistration(new(item));
        BlockRegistryListener?.OnRegistration(new(block));
        DimensionRegistryListener?.OnRegistration(new(dimensionType));
        ChatTypeRegistryListener?.OnRegistration(new(chatType));
        DamageTypeRegistryListener?.OnRegistration(new(damageType));
        BiomeRegistryListener?.OnRegistration(new(biome));
        PaintingVariantRegistryListener?.OnRegistration(new(paintingVariant));
        WolfVariantRegistryListener?.OnRegistration(new(wolfVariant));

        this.item = item.Freeze();
        this.block = block.Freeze();
        this.dimensionType = dimensionType.Freeze();
        this.chatType = chatType.Freeze();
        this.damageType = damageType.Freeze();
        this.biome = biome.Freeze();
        this.paintingVariant = paintingVariant.Freeze();
        this.wolfVariant = wolfVariant.Freeze();
        
        registries.Register(RegistryKeys.Item, this.item);
        registries.Register(RegistryKeys.Block, this.block);
        registries.Register(RegistryKeys.DimensionType, this.dimensionType);
        registries.Register(RegistryKeys.ChatType, this.chatType);
        registries.Register(RegistryKeys.DamageType, this.damageType);
        registries.Register(RegistryKeys.PaintingVariant, this.paintingVariant);
        registries.Register(RegistryKeys.WolfVariant, this.wolfVariant);
        registries.Register(RegistryKeys.Worldgen.Biome, this.biome);

        this.root = registries.Freeze();
        this.frozen = true;
    }
}
