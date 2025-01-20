using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Ink.Util;

namespace Ink.Blocks.State;

public abstract record EnumProperty : Property
{
    public abstract Type Enum { get; }
    public abstract IEnumerable<int> DefinedValues { get; }
}

public sealed record EnumProperty<TEnum>(FrozenDictionary<TEnum, int> Mapping, ImmutableArray<TEnum> Values) : EnumProperty
    where TEnum : unmanaged, System.Enum
{
    private readonly int bitsUsed = Utilities.BitsNeeded(Mapping.Count);
    public readonly FrozenDictionary<TEnum, int> Mapping = Mapping;
    public readonly ImmutableArray<TEnum> Values = Values;

    public override int PossibleValues => Mapping.Count;
    public override int BitsUsed => bitsUsed;

    public override Type Enum
        => typeof(TEnum);

    public override IEnumerable<int> DefinedValues
        => Values.Select(static e => e.GetHashCode());

    public override bool TryGetIndex<TValue>(TValue value, out int index)
    {
        if(value is not TEnum e
        || !Mapping.TryGetValue(e, out index))
        {
            index = default;
            return false;
        }

        return true;
    }

    public override bool TryGetValue<TValue>(int index, out TValue value)
    {
        if(typeof(TValue) != typeof(TEnum))
        {
            value = default;
            return false;
        }

        value = Unsafe.BitCast<TEnum, TValue>(Values[index]);
        return true;
    }

    public static EnumProperty<TEnum> From(params ReadOnlySpan<TEnum> values)
    {
        Dictionary<TEnum, int> mapping = new(values.Length);

        for(int i = 0; i < values.Length; ++i)
        {
            mapping.Add(values[i], i);
        }

        return new(mapping.ToFrozenDictionary(), values.ToImmutableArray());
    }
}
