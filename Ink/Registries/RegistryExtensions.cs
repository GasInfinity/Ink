namespace Ink.Registries;

public static class RegistryExtensions
{
    public static FrozenRegistryBuilder<IReadOnlyRegistry> Register<TValue>(this FrozenRegistryBuilder<IReadOnlyRegistry> registry, RegistryKey<IReadOnlyRegistry<TValue>> key, IReadOnlyRegistry<TValue> value)
        where TValue : class
    {
        registry.Register(key.Value, value); 
        return registry;
    }

    public static FrozenRegistryBuilder<TValue> Register<TValue>(this FrozenRegistryBuilder<TValue> registry, TValue value)
        where TValue : class, IHasLocation
    {
        if(value is IHasId)
        {
            registry.Register(((IHasId)value).Id, value.Location, value);
            return registry;
        }

        registry.Register(value.Location, value);
        return registry;
    }

    public static FrozenRegistryBuilder<TValue> Register<TValue>(this FrozenRegistryBuilder<TValue> registry, int id, TValue value)
        where TValue : class, IHasLocation
        => registry.Register(id, value.Location, value);
}
