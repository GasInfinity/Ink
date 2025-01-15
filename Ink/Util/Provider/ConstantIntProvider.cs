namespace Ink.Util.Provider;

public record ConstantIntProvider : IIntProvider
{
    public IntProviderType Type
        => IntProviderType.Constant;

    public int Constant { get; init; }

    public int Compute()
        => Constant;
}
