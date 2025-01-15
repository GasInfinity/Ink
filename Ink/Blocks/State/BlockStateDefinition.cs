using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Ink.Blocks.State;

public readonly record struct BlockStateDefinition(ImmutableArray<Property> Properties, int State, bool IsDefault)
{
    public readonly ImmutableArray<Property> Properties = Properties;
    public readonly int State = State;
    public readonly bool IsDefault = IsDefault;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeUniqueIndex(ImmutableArray<PropertyDefinition> definitions)
    {
        int raw = default;

        if (Properties.IsDefaultOrEmpty)
            return 0;

        int currentShift = 0;
        for(int i = 0; i < Properties.Length; ++i)
        {
            PropertyDefinition definition = definitions[i];
            Property property = Properties[i];

            raw |= ((property.Value - definition.Offset) << currentShift);
            currentShift += definition.BitsUsed;
        }

        return raw;
    }
}
