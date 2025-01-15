using Ink.Nbt;
using Ink.Nbt.Serialization;
using Ink.Registries;
using Ink.World.Biomes;

namespace Ink.Util.Converter.Nbt;

public sealed class NetworkBiomeTypeNbtConverter : NbtConverter<BiomeType>
{
    public static readonly NetworkBiomeTypeNbtConverter Shared = new();

    private NetworkBiomeTypeNbtConverter()
    {
    }

    public override BiomeType Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, BiomeType value)
    {
        writer.WriteCompoundStart();
        writer.WriteSByte("has_precipitation", value.HasPrecipitation);
        writer.WriteFloat("temperature", value.Temperature);

        if(value.TemperatureModifier != BiomeTemperatureModifier.None)
            writer.WriteString("temperature_modifier", value.TemperatureModifier.ToStringFast());

        writer.WriteFloat("downfall", value.Temperature);

        writer.WriteProperty(NbtTagType.Compound, "effects");
        WriteEffects(writer, value.Effects);

        writer.WriteCompoundEnd();
    }

    private static void WriteEffects<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, in BiomeEffects effects)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        writer.WriteInt("sky_color", effects.SkyColor);
        writer.WriteInt("water_fog_color", effects.WaterFogColor);
        writer.WriteInt("fog_color", effects.FogColor);
        writer.WriteInt("water_color", effects.WaterColor);

        if(effects.FoliageColor is int foliageColor)
            writer.WriteInt("foliage_color", foliageColor);

        if (effects.GrassColor is int grassColor)
            writer.WriteInt("grass_color", grassColor);

        if (effects.GrassColorModifier != BiomeGrassColorModifier.None)
            writer.WriteString("grass_color_modifier", effects.GrassColorModifier.ToStringFast());

        if(effects.Music is BiomeMusic music)
        {
            writer.WriteProperty(NbtTagType.Compound, "music");
            WriteMusic(writer, music);
        }

        if(effects.AmbientSound is Identifier ambientSound)
            writer.WriteString("ambient_sound", ambientSound.ToString()); // TODO! Span<char>!

        if (effects.AdditionsSound is BiomeAdditionsSound additionsSound)
        {
            writer.WriteProperty(NbtTagType.Compound, "additions_sound");
            WriteAdditionsSound(writer, additionsSound);
        }

        if (effects.MoodSound is BiomeMoodSound moodSound)
        {
            writer.WriteProperty(NbtTagType.Compound, "mood_sound");
            WriteMoodSound(writer, moodSound);
        }

        // TODO! Particles
        writer.WriteCompoundEnd();
    }

    private static void WriteMusic<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, in BiomeMusic music)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        writer.WriteSByte("replace_current_music", music.ReplaceCurrentMusic);
        writer.WriteString("sound", music.Sound.ToString()); // TODO! Span<char>!
        writer.WriteInt("max_delay", music.MaxDelay);
        writer.WriteInt("min_delay", music.MinDelay);
        writer.WriteCompoundEnd();
    }

    private static void WriteAdditionsSound<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, in BiomeAdditionsSound additions)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        writer.WriteString("sound", additions.Sound.ToString()); // TODO! Span<char>!
        writer.WriteDouble("tick_chance", additions.TickChance);
        writer.WriteCompoundEnd();
    }

    private static void WriteMoodSound<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, in BiomeMoodSound mood)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        writer.WriteString("sound", mood.Sound.ToString()); // TODO! Span<char>!
        writer.WriteInt("tick_delay", mood.TickDelay);
        writer.WriteDouble("offset", mood.Offset);
        writer.WriteInt("block_search_extent", mood.BlockSearchExtent);
        writer.WriteCompoundEnd();
    }
}
