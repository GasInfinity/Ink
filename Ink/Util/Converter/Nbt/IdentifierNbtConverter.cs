using Ink.Nbt;
using Ink.Nbt.Serialization;
using Ink.Registries;

namespace Ink.Util.Converter.Nbt;

public sealed class IdentifierNbtConverter : NbtConverter<Identifier>
{
    public override Identifier Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, Identifier value)
        => writer.WriteString(value.ToString()); // FIXME: Use Span<char>!
}
