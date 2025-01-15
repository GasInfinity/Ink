using Ink.Util;

namespace Ink.Blocks.State;

public readonly record struct PropertyDefinition(PropertyKind Kind, int Offset, int Max)
{
    public readonly byte Raw = Offset < 4 || Max < 16
                             ? (byte)(((uint)Kind << 6) | ((uint)Offset << 4) | ((uint)Utilities.BitSize(Max) & 0xF))
                             : throw new InvalidOperationException($"Cannot create a property with these parameters: {Offset} >= 4 || {Max} >= 16");

    public PropertyKind Kind
        => (PropertyKind)(Raw >>> 6);

    // TODO: This is a very hacky way of handling this...
    // HACK: Think about how to store possible property values...
    // FIXME: Yes, if mojang literally adds one beefy block, I'm cooked
    public int Offset
        => ((Raw >>> 4) & 0x3);

    public int BitsUsed
        => (Raw & 0xF);

    public int Max
        => (1 << BitsUsed) - 1;
}
