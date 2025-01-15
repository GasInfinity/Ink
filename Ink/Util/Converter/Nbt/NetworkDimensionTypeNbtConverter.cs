using Ink.Nbt;
using Ink.Nbt.Serialization;
using Ink.Registries;
using Ink.World;

namespace Ink.Util.Converter.Nbt;

public sealed class NetworkDimensionTypeNbtConverter : NbtConverter<DimensionType>
{
    public static readonly NetworkDimensionTypeNbtConverter Shared = new();

    private NetworkDimensionTypeNbtConverter()
    {
    }

    public override DimensionType Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, DimensionType value)
    {
        writer.WriteCompoundStart();

        writer.WriteSByte("ultrawarm", value.Ultrawarm);
        writer.WriteSByte("natural", value.Natural);
        writer.WriteDouble("coordinate_scale", value.CoordinateScale);
        writer.WriteSByte("has_skylight", value.HasSkylight);
        writer.WriteSByte("has_ceiling", value.HasCeiling);
        writer.WriteFloat("ambient_light", value.AmbientLight);

        writer.WriteProperty("monster_spawn_light_level");
        IntProviderNbtConverter.Shared.Write(writer, value.MonsterSpawnLightLevel);

        writer.WriteInt("monster_spawn_block_light_limit", value.MonsterSpawnBlockLightLimit);
        writer.WriteSByte("piglin_safe", value.PiglinSafe);
        writer.WriteSByte("bed_works", value.BedWorks);
        writer.WriteSByte("respawn_anchor_works", value.RespawnAnchorWorks);
        writer.WriteSByte("has_raids", value.HasRaids);
        writer.WriteInt("logical_height", value.LogicalHeight);
        writer.WriteInt("min_y", value.MinY);
        writer.WriteInt("height", value.Height);
        writer.WriteString("infiniburn", value.Infiniburn.ToString()); // FIXME! Span<char>!

        if (value.FixedTime is long fixedTime)
            writer.WriteLong("fixed_time", fixedTime);

        if (value.Effects is Tag effects)
            writer.WriteString("effects", effects.ToString()); // FIXME! Span<char>!

        writer.WriteCompoundEnd();
    }
}
