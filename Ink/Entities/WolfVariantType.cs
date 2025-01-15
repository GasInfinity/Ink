using Ink.Registries;

namespace Ink.Entities;

public sealed record WolfVariant(Identifier AngryTexture, Identifier WildTexture, Identifier TameTexture)
{
    public readonly Identifier AngryTexture = AngryTexture;
    public readonly Identifier WildTexture = WildTexture;
    public readonly Identifier TameTexture = TameTexture;
}
