using Ink.Nbt.Serialization;
using Ink.Nbt.Serialization.Metadata;
using Ink.Nbt.Tags;
using Ink.Text;
using Ink.Text.Serialization.Nbt;
using Ink.Util.Converter.Nbt;

namespace Ink.Util;

public sealed class InkNbtContext
{
    public static readonly NbtTypeInfo<TextPart> TextPart = new(new TextPartNbtConverter());
    public static readonly NbtTypeInfo<NbtTag> NbtTag = Ink.Nbt.Tags.NbtTag.TypeInfo;

    public static readonly NbtSerializerOptions DefaultNetworkOptions = new(128, true, true);

    static InkNbtContext()
    {
        DefaultNetworkOptions.AddConverter(TextPart.Converter);
        DefaultNetworkOptions.AddConverter(NbtTag.Converter);
        DefaultNetworkOptions.AddConverter(ChatTypeNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(DamageTypeNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(IntProviderNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(NetworkBiomeTypeNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(NetworkDimensionTypeNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(PaintingVariantNbtConverter.Shared);
        DefaultNetworkOptions.AddConverter(WolfVariantNbtConverter.Shared);
    }
}
