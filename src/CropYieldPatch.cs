using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

[HarmonyPatch(typeof(BlockCrop), nameof(BlockCrop.GetDrops))]
public class CropYieldPatch
{
    // Underground (artificial light ≥ 10, sunlight ≤ 2): +50% yield
    // Surface (sunlight ≥ 13): −30% yield
    [HarmonyPostfix]
    public static void Postfix(BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref BlockDropItemStack[] __result)
    {
        if (__result == null || __result.Length == 0) return;
        if (world.BlockAccessor.GetBlockEntity(pos.DownCopy()) is not BlockEntityFarmland) return;

        int sunLight = world.BlockAccessor.GetLightLevel(pos, EnumLightLevelType.OnlySunLight);
        int totalLight = world.BlockAccessor.GetLightLevel(pos, EnumLightLevelType.MaxLight);
        int artificialLight = Math.Max(0, totalLight - sunLight);

        float multiplier;
        if (sunLight >= 13)
            multiplier = 0.7f;
        else if (artificialLight >= 10 && sunLight <= 2)
            multiplier = 1.5f;
        else
            return;

        foreach (var drop in __result)
        {
            var q = drop.Quantity;
            q.avg *= multiplier;
            q.var *= multiplier;
            drop.Quantity = q;
        }
    }
}
