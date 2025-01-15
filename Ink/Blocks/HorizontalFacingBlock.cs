using Ink.Blocks.State;
using Ink.Items;

namespace Ink.Blocks;

public class HorizontalFacingBlock : Block
{
    public HorizontalFacingBlock(BlockStateRoot stateRoot, Settings settings) : base(stateRoot, settings)
    {
    }

    public override BlockStateChild ComputePlacementState(in ItemPlacementContext context)
        => DefaultState.WithProperty(Properties.Facing, context.HorizontalPlayerDirection.Opposite.Horizontal);
}
