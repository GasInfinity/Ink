using System.Runtime.CompilerServices;
using Ink.Util;

namespace Ink.Blocks.State;

public sealed record Int32Property(int Min, int Max) : Property
{
    private readonly int bitsUsed = Utilities.BitsNeeded(Max - Min);
    private readonly int possibleValues = Max - Min;
    public readonly int Min = Min;
    public readonly int Max = Max;

    public override int PossibleValues => possibleValues;
    public override int BitsUsed => bitsUsed;

    public override bool TryGetIndex<TValue>(TValue value, out int index)
    {
        if(value is not int int32
        || int32 < Min 
        || int32 > Max)
        {
            index = default;
            return false;
        }

        index = int32 - Min;
        return true;
    }

    public override bool TryGetValue<TValue>(int index, out TValue value)
    {
        Unsafe.SkipInit(out value);
        if(typeof(TValue) == typeof(int))
        {
            Unsafe.As<TValue, int>(ref value) = index + Min;
            return true;
        }

        value = default;
        return false;
    }
}
