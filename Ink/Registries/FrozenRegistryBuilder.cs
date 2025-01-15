namespace Ink.Registries;

public readonly record struct FrozenRegistryBuilder<TValue>
    where TValue : class
{
    private readonly Dictionary<Identifier, int> keyId = [];
    private readonly Dictionary<int, Identifier> idKey = [];
    private readonly Dictionary<Identifier, TValue> mapping = [];

    public int Count
        => this.mapping?.Count ?? 0;

    public FrozenRegistryBuilder()
    {
    }

    public TValue? Get(Identifier key)
    {
        if (mapping.TryGetValue(key, out var value))
            return value;

        return null;
    }

    public TValue? Get(int id)
    {
        if (idKey.TryGetValue(id, out Identifier key) && mapping.TryGetValue(key, out var value))
            return value;

        return null;
    }

    public int GetId(Identifier key)
    {
        if (this.keyId.TryGetValue(key, out int id))
            return id;

        return -1;
    }

    public bool TryGetId(Identifier key, out int id)
    {
        if (this.keyId.TryGetValue(key, out id))
            return true;

        return false;
    }

    public FrozenRegistryBuilder<TValue> Register(Identifier key, TValue value)
    {
        int newId = Count;

        while (this.idKey.ContainsKey(newId))
            newId++; // Find next empty id

        if (!this.mapping.TryAdd(key, value))
            throw new InvalidOperationException($"Trying to register a value with key '{key}' but that key was already registered to other value.");

        _ = this.idKey.TryAdd(newId, key);
        _ = this.keyId.TryAdd(key, newId);
        return this;
    }

    public FrozenRegistryBuilder<TValue> Register(int id, Identifier key, TValue value)
    {
        if (this.idKey.TryGetValue(id, out Identifier lastKey))
            throw new InvalidOperationException($"Trying to register a value with key '{key}' and '{id}'. That id was already registered with other key '{lastKey}'");

        if (!this.mapping.TryAdd(key, value))
            throw new InvalidOperationException($"Trying to register a value with key '{key}' but that key was already registered to other value.");

        _ = this.idKey.TryAdd(id, key);
        _ = this.keyId.TryAdd(key, id);
        return this;
    }

    public FrozenRegistry<TValue> Freeze()
        => Count > 0 ? new(this.keyId, this.idKey, this.mapping) : FrozenRegistry<TValue>.Empty;

    public Dictionary<Identifier, TValue>.Enumerator GetEnumerator()
        => this.mapping.GetEnumerator();
}
