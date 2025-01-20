using Ink.Blocks.State;
using Ink.Items;

namespace Ink.Blocks;

public class HorizontalFacingBlock : Block
{
    public HorizontalFacingBlock(BlockStateRoot stateRoot, Settings settings) : base(stateRoot, settings)
    {
    }

    public override BlockState ComputePlacementState(in ItemPlacementContext context)
        => DefaultState.WithProperty(PropertyNames.Facing, context.HorizontalPlayerDirection.Opposite.Value);
}
