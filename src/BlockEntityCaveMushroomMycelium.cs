using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Mineholme;

public class BlockEntityCaveMushroomMycelium : BlockEntity
{
    string? mushroomType;
    double regrowAtTotalHours;
    const double RegrowthHours = 48.0;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side == EnumAppSide.Server)
            RegisterGameTickListener(OnTick, 60_000);
    }

    public void SetMushroomType(string type)
    {
        mushroomType = type;
        regrowAtTotalHours = Api.World.Calendar.TotalHours + RegrowthHours;
        MarkDirty(true);
    }

    void OnTick(float dt)
    {
        if (mushroomType == null) return;
        if (Api.World.Calendar.TotalHours < regrowAtTotalHours) return;

        Block? grown = Api.World.GetBlock(new AssetLocation("mineholme", "cave-mushroom-planted-" + mushroomType));
        if (grown == null) return;

        // Check there's still space (nothing placed on top)
        Block above = Api.World.BlockAccessor.GetBlock(Pos.UpCopy());
        if (above.BlockMaterial != EnumBlockMaterial.Air) return;

        Api.World.BlockAccessor.SetBlock(grown.BlockId, Pos);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        mushroomType = tree.GetString("mushroomType");
        regrowAtTotalHours = tree.GetDouble("regrowAtTotalHours");
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetString("mushroomType", mushroomType ?? "");
        tree.SetDouble("regrowAtTotalHours", regrowAtTotalHours);
    }
}
