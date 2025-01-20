using Ink.Blocks.State;
using Ink.Items;
using Ink.Math;

namespace Ink.Blocks;

public class AxisFacingBlock : Block
{
    public AxisFacingBlock(BlockStateRoot stateRoot, Settings settings) : base(stateRoot, settings)
    {
    }

    public override BlockState ComputePlacementState(in ItemPlacementContext context)
        => DefaultState.WithProperty(PropertyNames.Axis, context.PlaceFace.ToDirection().Axis);
}
