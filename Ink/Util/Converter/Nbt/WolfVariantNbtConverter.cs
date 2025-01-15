using Ink.Entities;
using Ink.Nbt;
using Ink.Nbt.Serialization;

namespace Ink.Util.Converter.Nbt;

public sealed class WolfVariantNbtConverter : NbtConverter<WolfVariant>
{
    public static readonly WolfVariantNbtConverter Shared = new();

    private WolfVariantNbtConverter()
    {
    }

    public override WolfVariant Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, WolfVariant value)
    {
        writer.WriteCompoundStart();
        writer.WriteString("angry_texture", value.AngryTexture.ToString());
        writer.WriteString("wild_texture", value.WildTexture.ToString());
        writer.WriteString("tame_texture", value.TameTexture.ToString());
        writer.WriteString("biomes", "plains"); // FIXME: Registry tags / references
        writer.WriteCompoundEnd();
    }
}
