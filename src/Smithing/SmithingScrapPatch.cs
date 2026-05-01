using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

// Tracks how many voxels the player has placed onto the anvil across all ingot additions,
// then compares that total to the recipe requirement when a craft completes.
// Excess voxels become metal bits spawned at the anvil.

// ── CheckIfFinished patches ──────────────────────────────────────────────────

[HarmonyPatch(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.CheckIfFinished))]
static class AnvilCheckIfFinishedPatch
{
    // State carries what we need from BEFORE the recipe clears workItemStack/SelectedRecipe.
    internal sealed class FinishState
    {
        public required ItemStack WorkItem;
        public required SmithingRecipe Recipe;
    }

    [HarmonyPrefix]
    static void Prefix(BlockEntityAnvil __instance, ref FinishState? __state)
    {
        __state = null;
        if (__instance.WorkItemStack == null || __instance.SelectedRecipe == null) return;

        __state = new FinishState
        {
            WorkItem = __instance.WorkItemStack.Clone(),
            Recipe   = __instance.SelectedRecipe
        };
    }

    [HarmonyPostfix]
    static void Postfix(BlockEntityAnvil __instance, IPlayer byPlayer, FinishState? __state)
    {
        // Only run when: state captured, recipe completed (workItemStack is now null), server side.
        if (__state == null) return;
        if (__instance.WorkItemStack != null) return; // recipe did not complete
        if (__instance.Api.World.Side != EnumAppSide.Server) return;

        // Count recipe voxels.
        int recipeVoxels = 0;
        bool[,,] rv = __state.Recipe.Voxels;
        int lx = rv.GetLength(0), ly = rv.GetLength(1), lz = rv.GetLength(2);
        for (int x = 0; x < lx; x++)
            for (int y = 0; y < ly; y++)
                for (int z = 0; z < lz; z++)
                    if (rv[x, y, z]) recipeVoxels++;

        int totalPlaced = __state.WorkItem.Attributes.GetInt("mh_voxelsPlaced", recipeVoxels);
        int scrappedVoxels = Math.Max(0, totalPlaced - recipeVoxels);

        // 42 voxels = 1 ingot = 100 units; 1 bit = 5 units → 2.1 voxels per bit
        int bits = (int)Math.Floor(scrappedVoxels / 2.1);
        if (bits <= 0) return;

        string? metal = __state.WorkItem.Collectible.Variant["metal"];
        if (string.IsNullOrEmpty(metal)) return;

        Item? bitItem = __instance.Api.World.GetItem(new AssetLocation($"game:metalbit-{metal}"));
        if (bitItem == null) return;

        var bitStack = new ItemStack(bitItem, bits);
        float temp = __state.WorkItem.Collectible.GetTemperature(__instance.Api.World, __state.WorkItem);
        bitItem.SetTemperature(__instance.Api.World, bitStack, temp);

        __instance.Api.World.SpawnItemEntity(bitStack,
            __instance.Pos.ToVec3d().Add(0.5, 0.75, 0.5));
    }
}

// ── ItemIngot.TryPlaceOn postfix — track voxels placed ──────────────────────

[HarmonyPatch(typeof(ItemIngot), nameof(ItemIngot.TryPlaceOn))]
static class ItemIngotTryPlaceOnPatch
{
    [HarmonyPostfix]
    static void Postfix(ItemStack stack, BlockEntityAnvil beAnvil, ItemStack? __result)
    {
        if (__result == null) return;

        // On first placement WorkItemStack is still null at postfix time (TryPut sets it after);
        // __result is the new work item in that case.  On subsequent placements WorkItemStack
        // already holds it.  We update whichever is the live work item on the anvil.
        ItemStack target = beAnvil.WorkItemStack ?? __result;

        int prev = target.Attributes.GetInt("mh_voxelsPlaced", 0);
        target.Attributes.SetInt("mh_voxelsPlaced", prev + ItemIngot.VoxelCount);
    }
}

// ── ItemIronBloom.TryPlaceOn postfix — track voxels placed ──────────────────

[HarmonyPatch(typeof(ItemIronBloom), nameof(ItemIronBloom.TryPlaceOn))]
static class ItemIronBloomTryPlaceOnPatch
{
    [HarmonyPostfix]
    static void Postfix(ItemStack stack, BlockEntityAnvil beAnvil, ItemStack? __result)
    {
        if (__result == null) return;

        // Iron bloom places itself as the work item; update the result directly.
        ItemStack target = beAnvil.WorkItemStack ?? __result;

        int prev = target.Attributes.GetInt("mh_voxelsPlaced", 0);
        target.Attributes.SetInt("mh_voxelsPlaced", prev + ItemIngot.VoxelCount);
    }
}
