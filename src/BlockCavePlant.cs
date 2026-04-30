using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Mineholme;

public class BlockCavePlant : Block
{
    public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldGenRand, BlockPatchAttributes attributes = null)
    {
        Block blockBelow = blockAccessor.GetBlock(pos.DownCopy());
        if (!blockBelow.SideSolid[BlockFacing.UP.Index]) return false;
        return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand, attributes);
    }
}
