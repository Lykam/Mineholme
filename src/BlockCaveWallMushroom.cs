using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Mineholme;

public class BlockCaveWallMushroom : Block
{
    public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldGenRand, BlockPatchAttributes? attributes = null)
    {
        if (!blockAccessor.GetBlock(pos).IsReplacableBy(this)) return false;

        // Randomize which wall direction we try first
        BlockFacing[] horizontals = { BlockFacing.NORTH, BlockFacing.EAST, BlockFacing.SOUTH, BlockFacing.WEST };
        for (int i = horizontals.Length - 1; i > 0; i--)
        {
            int j = worldGenRand.NextInt(i + 1);
            (horizontals[i], horizontals[j]) = (horizontals[j], horizontals[i]);
        }

        foreach (BlockFacing wallDir in horizontals)
        {
            Block neighbor = blockAccessor.GetBlock(pos.AddCopy(wallDir));
            if (!neighbor.SideSolid[wallDir.Opposite.Index]) continue;

            // The face opposite the wall must be open (mushroom needs space to hang out)
            Block frontBlock = blockAccessor.GetBlock(pos.AddCopy(wallDir.Opposite));
            if (!frontBlock.IsReplacableBy(this)) continue;

            Block variant = blockAccessor.GetBlock(CodeWithVariant("side", wallDir.Code));
            if (variant == null || variant.Id == 0) continue;

            blockAccessor.SetBlock(variant.BlockId, pos);
            return true;
        }

        return false;
    }
}
