using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

[HarmonyPatch(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetHoursForNextStage))]
public class FarmlandGrowthPatch
{
    // Artificial light ≥ 10 with sunlight ≤ 2: underground bonus (1.5× growth speed)
    // Sunlight ≥ 13: surface penalty (1.5× slower)
    // Anything else: no modifier
    [HarmonyPostfix]
    public static void Postfix(BlockEntityFarmland __instance, ref double __result)
    {
        var world = __instance.Api.World;
        BlockPos cropPos = __instance.Pos.UpCopy();

        int sunLight = world.BlockAccessor.GetLightLevel(cropPos, EnumLightLevelType.OnlySunLight);
        int totalLight = world.BlockAccessor.GetLightLevel(cropPos, EnumLightLevelType.MaxLight);
        int artificialLight = Math.Max(0, totalLight - sunLight);

        if (sunLight >= 13)
            __result *= 1.5;   // surface penalty
        else if (artificialLight >= 10 && sunLight <= 2)
            __result *= 0.667; // underground bonus
    }
}
