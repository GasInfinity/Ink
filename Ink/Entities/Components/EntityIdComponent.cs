using Friflo.Engine.ECS;
using Ink.Util;

namespace Ink.Entities.Components;

public readonly record struct EntityIdComponent(int NetworkId, Uuid Id) : IIndexedComponent<int>, IIndexedComponent<Uuid>
{
    public readonly int NetworkId = NetworkId;
    public readonly Uuid Id = Id;

    int IIndexedComponent<int>.GetIndexedValue() => NetworkId;
    Uuid IIndexedComponent<Uuid>.GetIndexedValue() => Id;
}
