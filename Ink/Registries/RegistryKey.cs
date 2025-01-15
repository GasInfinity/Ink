namespace Ink.Registries;

// HACK: Why doesnt c# support covariance on immutable structs...
public readonly record struct RegistryKey(Identifier Registry, Identifier Value)
{
    public readonly Identifier Registry = Registry;
    public readonly Identifier Value = Value;
}

public readonly record struct RegistryKey<TValue>(Identifier Registry, Identifier Value)
{
    public readonly Identifier Registry = Registry;
    public readonly Identifier Value = Value;

    public static implicit operator RegistryKey(RegistryKey<TValue> value)
        => new(value.Registry, value.Value);
}
