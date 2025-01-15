using System.Collections.Immutable;

namespace Ink.Registries;

public interface IReadOnlyRegistry
{
    Type Element { get; }
    ImmutableArray<Identifier> Keys { get; }

    int Count { get; }

    object? Get(Identifier key);
    object? Get(int id);

    int GetId(Identifier key);
    bool TryGetId(Identifier key, out int id);
}
