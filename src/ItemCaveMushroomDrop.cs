using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Mineholme;

public class ItemCaveMushroomDrop : Item
{
    static readonly EnumBlockMaterial[] ValidSurfaces = { EnumBlockMaterial.Stone, EnumBlockMaterial.Soil };

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        if (blockSel == null || blockSel.Face != BlockFacing.UP) return;

        IWorldAccessor world = byEntity.World;
        BlockPos placePos = blockSel.Position.UpCopy();

        Block blockBelow = world.BlockAccessor.GetBlock(blockSel.Position);
        string mushroomType = Code.Path.Replace("cave-mushroom-drop-", "");

        // Farmland check must come before SideSolid — farmland has up: false but is still plantable
        if (blockBelow.Code.Path.StartsWith("farmland-"))
        {
            if (world.BlockAccessor.GetRainMapHeightAt(placePos.X, placePos.Z) <= placePos.Y) return;
            if (world.BlockAccessor.GetBlock(placePos).BlockMaterial != EnumBlockMaterial.Air) return;
            handling = EnumHandHandling.PreventDefault;
            if (world.Side == EnumAppSide.Server)
            {
                Block? cropBlock = world.GetBlock(new AssetLocation("mineholme", "cave-mushroom-crop-" + mushroomType + "-1"));
                if (cropBlock != null)
                {
                    world.BlockAccessor.SetBlock(cropBlock.BlockId, placePos);
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }
            return;
        }

        // Stone or soil → solid face required
        if (!blockBelow.SideSolid[BlockFacing.UP.Index]) return;
        if (world.BlockAccessor.GetRainMapHeightAt(placePos.X, placePos.Z) <= placePos.Y) return;
        if (world.BlockAccessor.GetBlock(placePos).BlockMaterial != EnumBlockMaterial.Air) return;
        if (Array.IndexOf(ValidSurfaces, blockBelow.BlockMaterial) < 0) return;

        handling = EnumHandHandling.PreventDefault;
        if (world.Side == EnumAppSide.Server)
        {
            Block? mushroomBlock = world.GetBlock(new AssetLocation("mineholme", "cave-mushroom-planted-" + mushroomType));
            if (mushroomBlock == null) return;
            world.BlockAccessor.SetBlock(mushroomBlock.BlockId, placePos);
            slot.TakeOut(1);
            slot.MarkDirty();
        }
    }
}
