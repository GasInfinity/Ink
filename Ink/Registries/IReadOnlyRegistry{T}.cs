namespace Ink.Registries;

public interface IReadOnlyRegistry<out TValue> : IReadOnlyRegistry
{
    Type IReadOnlyRegistry.Element => typeof(TValue);

    new TValue? Get(Identifier key);
    new TValue? Get(int id);

    new int GetId(Identifier key);
    new bool TryGetId(Identifier key, out int id);

    object? IReadOnlyRegistry.Get(Identifier key) => Get(key);
    object? IReadOnlyRegistry.Get(int id) => Get(id);

    int IReadOnlyRegistry.GetId(Identifier key) => GetId(key);
    bool IReadOnlyRegistry.TryGetId(Identifier key, out int id) => TryGetId(key, out id);
}
