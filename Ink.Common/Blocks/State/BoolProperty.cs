using System.Runtime.CompilerServices;

namespace Ink.Blocks.State;

public sealed record BoolProperty : Property
{
    public static readonly BoolProperty Shared = new();

    public override int PossibleValues => 2;
    public override int BitsUsed => 1;

    private BoolProperty()
    { }

    public override bool TryGetIndex<TValue>(TValue value, out int index)
    {
        if(value is bool boolValue)
        {
            index = Unsafe.BitCast<bool, byte>(!boolValue);
            return true;
        }

        index = default;
        return false;
    }

    public override bool TryGetValue<TValue>(int index, out TValue value)
    {
        if(typeof(TValue) != typeof(bool))
        {
            value = default;
            return false;
        }

        Unsafe.SkipInit(out value);
        Unsafe.As<TValue, bool>(ref value) = !Unsafe.BitCast<byte, bool>((byte)index); // Wtf, mojang, why true is 1 and false is 0 in blocks.json? Were you drunk?? :skull:
        return true;
    }
}
