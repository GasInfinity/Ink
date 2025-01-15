using Ink.Server.Entities;
using Ink.World;

namespace Ink.Server.Event;

public record struct LoginEvent(ServerPlayerEntity Player)
{
    public readonly ServerPlayerEntity Player = Player;
    public BaseWorld? AssignedWorld = null;
}
