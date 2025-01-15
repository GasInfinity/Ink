using Ink.Nbt.Tags;

namespace Ink.Util.Provider;

public interface IIntProvider
{
    public static readonly ConstantIntProvider Zero = new() { Constant = 0 };

    IntProviderType Type { get; }

    int Compute();
}
