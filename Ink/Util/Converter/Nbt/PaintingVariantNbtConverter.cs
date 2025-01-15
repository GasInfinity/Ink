using Ink.Blocks;
using Ink.Nbt;
using Ink.Nbt.Serialization;

namespace Ink.Util.Converter.Nbt;

public sealed class PaintingVariantNbtConverter : NbtConverter<PaintingVariant>
{
    public static readonly PaintingVariantNbtConverter Shared = new();

    private PaintingVariantNbtConverter ()
    {
    }

    public override PaintingVariant Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, PaintingVariant value)
    {
        writer.WriteCompoundStart();
        writer.WriteString("asset_id", value.AssetId.ToString());
        writer.WriteInt("width", value.Width);
        writer.WriteInt("height", value.Height);
        writer.WriteCompoundEnd();
    }
}
