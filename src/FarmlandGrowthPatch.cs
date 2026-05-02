using System;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

[HarmonyPatch(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetHoursForNextStage))]
public class FarmlandGrowthPatch
{
    // Artificial light ≥ 10 with sunlight ≤ 2: underground bonus (~25% shorter cycle)
    // Sunlight ≥ 13: surface penalty (~20% longer cycle)
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
            __result *= 1.2;  // surface penalty
        else if (artificialLight >= 10 && sunLight <= 2)
            __result *= 0.75; // underground bonus
    }
}

[HarmonyPatch(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetBlockInfo))]
public class FarmlandTooltipPatch
{
    [HarmonyPostfix]
    public static void Postfix(BlockEntityFarmland __instance, IPlayer forPlayer, StringBuilder dsc)
    {
        if (__instance.Api.Side != EnumAppSide.Client) return;

        var world = __instance.Api.World;
        BlockPos cropPos = __instance.Pos.UpCopy();

        int sunLight = world.BlockAccessor.GetLightLevel(cropPos, EnumLightLevelType.OnlySunLight);
        int totalLight = world.BlockAccessor.GetLightLevel(cropPos, EnumLightLevelType.MaxLight);
        int artificialLight = Math.Max(0, totalLight - sunLight);

        string? line = sunLight >= 13
            ? Lang.Get("mineholme:farmland-surface-penalty")
            : (artificialLight >= 10 && sunLight <= 2 ? Lang.Get("mineholme:farmland-underground-bonus") : null);

        if (line != null && !dsc.ToString().Contains(line))
            dsc.AppendLine(line);
    }
}

// Patch the growth tick (Update) so allowundergroundfarming reflects actual light at tick time,
// not the static world-config value set at Initialize. UpdateFarmlandBlock is the visual-state
// method and runs independently — patching it doesn't affect the growth simulation.
[HarmonyPatch(typeof(BlockEntityFastForwardGrowth), "Update")]
public class FarmlandLightFactorPatch
{
    [HarmonyPrefix]
    public static void Prefix(BlockEntityFastForwardGrowth __instance)
    {
        var world = __instance.Api.World;
        BlockPos upPos = Traverse.Create(__instance).Field<BlockPos>("upPos").Value;
        if (upPos == null) return;

        int sunLight = world.BlockAccessor.GetLightLevel(upPos, EnumLightLevelType.OnlySunLight);
        int totalLight = world.BlockAccessor.GetLightLevel(upPos, EnumLightLevelType.MaxLight);
        int artificialLight = Math.Max(0, totalLight - sunLight);

        Traverse.Create(__instance).Field("allowundergroundfarming").SetValue(artificialLight >= 10 && sunLight <= 2);
    }
}
