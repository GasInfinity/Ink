using Ink.Registries;

namespace Ink.Event.Registry;

public readonly record struct ValueRegistrationEvent<TValue>(FrozenRegistryBuilder<TValue> Registry)
    where TValue : class
{
    public readonly FrozenRegistryBuilder<TValue> Registry = Registry;
}
