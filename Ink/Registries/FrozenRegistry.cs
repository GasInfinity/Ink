using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Ink.Registries;

public sealed class FrozenRegistry<TValue> : IReadOnlyRegistry<TValue>
    where TValue : class
{
    public static readonly FrozenRegistry<TValue> Empty = new();

    private readonly FrozenDictionary<Identifier, int> keyId;
    private readonly FrozenDictionary<int, Identifier> idKey;
    private readonly FrozenDictionary<Identifier, TValue> mapping;

    public ImmutableArray<Identifier> Keys
        => this.mapping.Keys;

    public int Count
        => this.mapping.Count;

    public ImmutableArray<int> Ids
        => this.idKey.Keys;

    private FrozenRegistry()
        => (this.keyId, this.idKey, this.mapping) = (FrozenDictionary<Identifier, int>.Empty, FrozenDictionary<int, Identifier>.Empty, FrozenDictionary<Identifier, TValue>.Empty);

    public FrozenRegistry(Dictionary<Identifier, int> keyId, Dictionary<int, Identifier> idKey, Dictionary<Identifier, TValue> mapping)
    {
        this.keyId = keyId.ToFrozenDictionary();
        this.idKey = idKey.ToFrozenDictionary();
        this.mapping = mapping.ToFrozenDictionary();
    }

    public TValue? Get(Identifier key)
    {
        if(mapping.TryGetValue(key, out var value))
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
        => TryGetId(key, out int id) ? id : -1;

    public bool TryGetId(Identifier key, out int id)
        => this.keyId.TryGetValue(key, out id);

    public FrozenDictionary<Identifier, TValue>.Enumerator GetEnumerator()
        => this.mapping.GetEnumerator();
}
