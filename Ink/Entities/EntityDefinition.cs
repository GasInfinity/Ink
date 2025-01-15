using Ink.Data;
using Ink.Math;

namespace Ink.Entities;

public record EntityDefinition
{
    public static readonly EntityDefinition Player = new()
    {
        Type = EntityType.Player,
        TickSyncInterval = 2,
        CollisionBox = AabbDefinition.FromSizes(0.6, 1.8),
        EyeHeight = 1.62
    };

    public static readonly EntityDefinition Cow = new()
    {
        Type = EntityType.Cow,
        TickSyncInterval = 2,
        CollisionBox = AabbDefinition.FromSizes(0.9, 1.4),
        EyeHeight = 1.3
    };

    public static readonly EntityDefinition Item = new()
    {
        Type = EntityType.Item,
        TickSyncInterval = 4,
        ObstructsBlockPlacements = false,
        CollisionBox = AabbDefinition.FromSizes(0.25, 0.25),
        EyeHeight = 0.2125,
        DragBeforeAcceleration = true,
        GravityAcceleration = 0.04
    };

    public int Type { get; init; }
    public uint TickSyncInterval { get; init; }
    public bool ConsistentSyncUpdates { get; init; } = false;
    public bool ObstructsBlockPlacements { get; init; } = true;
    public AabbDefinition CollisionBox { get; init; } = AabbDefinition.Empty;
    public double EyeHeight { get; init; } = 0;
    public bool DragBeforeAcceleration { get; init; } = false;
    public double GravityAcceleration { get; init; } = 0.08;
    public double VerticalDrag { get; init; } = 0.98;
}
