using Ink.Registries;
using Ink.Util.Provider;

namespace Ink.Worlds;

public sealed record DimensionType
{
    // TODO! Load these from JSON!!
    public static readonly DimensionType Overworld = new()
    {
        AmbientLight = 0,
        BedWorks = true,
        CoordinateScale = 1,
        Effects = Identifier.Vanilla("overworld"),
        HasCeiling = false,
        HasRaids = true,
        HasSkylight = true,
        Height = 384,
        Infiniburn = Identifier.Vanilla("infiniburn_overworld"),
        LogicalHeight = 384,
        MinY = -64,
        MonsterSpawnBlockLightLimit = 0,
        MonsterSpawnLightLevel = IIntProvider.Zero,
        Natural = true,
        PiglinSafe = false,
        RespawnAnchorWorks = false,
        Ultrawarm = false,
    };

    public static readonly DimensionType Nether = new()
    {
        AmbientLight = 0.1f,
        BedWorks = false,
        CoordinateScale = 8,
        Effects = Identifier.Vanilla("the_nether"),
        HasCeiling = true,
        HasRaids = false,
        HasSkylight = false,
        Height = 256,
        Infiniburn = Identifier.Vanilla("infiniburn_nether"),
        LogicalHeight = 128,
        MinY = 0,
        MonsterSpawnBlockLightLimit = 15,
        MonsterSpawnLightLevel = new ConstantIntProvider() { Constant = 7 },
        Natural = false,
        PiglinSafe = true,
        RespawnAnchorWorks = true,
        Ultrawarm = true,
    };

    public static readonly DimensionType End = new()
    {
        AmbientLight = 0,
        BedWorks = false,
        CoordinateScale = 1,
        Effects = Identifier.Vanilla("the_end"),
        HasCeiling = false,
        HasRaids = true,
        HasSkylight = false,
        Height = 256,
        Infiniburn = Identifier.Vanilla("infiniburn_end"),
        LogicalHeight = 256,
        MinY = 0,
        MonsterSpawnBlockLightLimit = 0,
        MonsterSpawnLightLevel = IIntProvider.Zero,
        Natural = false,
        PiglinSafe = false,
        RespawnAnchorWorks = false,
        Ultrawarm = false,
    };

    public bool Ultrawarm { get; init; }
    public bool Natural { get; init; }
    public double CoordinateScale { get; init; }
    public bool HasSkylight { get; init; }
    public bool HasCeiling { get; init; }
    public float AmbientLight { get; init; }
    public long? FixedTime { get; init; }
    public IIntProvider MonsterSpawnLightLevel { get; init; } = IIntProvider.Zero;
    public int MonsterSpawnBlockLightLimit { get; init; }
    public bool PiglinSafe { get; init; }
    public bool BedWorks { get; init; }
    public bool RespawnAnchorWorks { get; init; }
    public bool HasRaids { get; init; }
    public int LogicalHeight { get; init; }
    public int MinY { get; init; }
    public int Height { get; init; }
    public Tag Infiniburn { get; init; } = Identifier.Vanilla("infiniburn_overworld");
    public Tag Effects { get; init; } = Identifier.Vanilla("overworld");

    public DimensionType()
    {
    }
}
