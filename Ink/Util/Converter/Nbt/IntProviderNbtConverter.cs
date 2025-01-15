using Ink.Nbt;
using Ink.Nbt.Serialization;
using Ink.Util.Provider;

namespace Ink.Util.Converter.Nbt;

public sealed class IntProviderNbtConverter : NbtConverter<IIntProvider>
{
    public static readonly IntProviderNbtConverter Shared = new ();

    private IntProviderNbtConverter()
    {
    }

    public override IIntProvider Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, IIntProvider value)
    {
        switch (value)
        {
            case ConstantIntProvider cProvider:
                {
                    writer.WriteInt(cProvider.Constant);
                    break;
                }
            default: // Should never happen
                {
                    break;
                }
        }
    }
}
