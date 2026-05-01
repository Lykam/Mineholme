using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Mineholme;

// Player-planted cave mushrooms on stone leave a mycelium stub when broken
// so they can regrow. Extends BlockCavePlant to keep worldgen placement rules.
public class BlockCaveMushroomFloor : BlockCavePlant
{
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        string mushroomType = Code.Path.Replace("cave-mushroom-planted-", "");

        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

        if (byPlayer == null || world.Side != EnumAppSide.Server) return;

        Block blockBelow = world.BlockAccessor.GetBlock(pos.DownCopy());
        if (blockBelow.BlockMaterial != EnumBlockMaterial.Stone) return;

        Block? mycelium = world.GetBlock(new AssetLocation("mineholme", "cave-mushroom-mycelium"));
        if (mycelium == null) return;

        world.BlockAccessor.SetBlock(mycelium.BlockId, pos);
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityCaveMushroomMycelium be)
            be.SetMushroomType(mushroomType);
    }
}
