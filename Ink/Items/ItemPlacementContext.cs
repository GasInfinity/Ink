using Ink.Entities;
using Ink.Math;

namespace Ink.Items;

public readonly record struct ItemPlacementContext(PlayerEntity PlacingPlayer, BlockFace PlaceFace, float CursorX, float CursorY, float CursorZ)
{
    public readonly PlayerEntity PlacingPlayer = PlacingPlayer;
    public readonly BlockFace PlaceFace = PlaceFace;
    public readonly float CursorX = CursorX;
    public readonly float CursorY = CursorY;
    public readonly float CursorZ = CursorZ;

    public Direction HorizontalPlayerDirection
        => PlacingPlayer.HorizontalDirection;

    public Direction PlayerDirection
        => PlacingPlayer.Direction;
}
