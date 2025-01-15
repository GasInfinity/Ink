using Ink.Registries;

namespace Ink.Blocks;

public sealed record PaintingVariant(Identifier AssetId, int Width = 1, int Height = 1)
{
    public Identifier AssetId = AssetId;
    public int Width = Width;
    public int Height = Height;
}
