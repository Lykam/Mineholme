using System;
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

// Tracks how many voxels the player has placed onto the anvil across all ingot additions,
// then compares that total to the recipe requirement when a craft completes.
// Excess voxels become metal bits spawned at the anvil.
//
// Singleplayer double-call pattern: VS fires TryPlaceOn and OnSplit on the same server-side
// BlockEntityAnvil twice per player action — once from the client code path and once from
// the server packet handler. Both calls have World.Side == Server on the shared instance.
// Each patch that increments a counter must dedup within a single tick (~50 ms window).

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

    static readonly Dictionary<int, long> _lastScrapMs = new();

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
        if (__state == null) return;
        if (__instance.WorkItemStack != null) return; // recipe did not complete
        if (__instance.Api.World.Side != EnumAppSide.Server) return;

        int posKey = __instance.Pos.GetHashCode();
        long nowMs = __instance.Api.World.ElapsedMilliseconds;
        if (_lastScrapMs.TryGetValue(posKey, out long lastMs) && nowMs - lastMs < 500)
            return;
        _lastScrapMs[posKey] = nowMs;

        // Count recipe voxels.
        int recipeVoxels = 0;
        bool[,,] rv = __state.Recipe.Voxels;
        int lx = rv.GetLength(0), ly = rv.GetLength(1), lz = rv.GetLength(2);
        for (int x = 0; x < lx; x++)
            for (int y = 0; y < ly; y++)
                for (int z = 0; z < lz; z++)
                    if (rv[x, y, z]) recipeVoxels++;

        int totalPlaced    = __state.WorkItem.Attributes.GetInt("mh_voxelsPlaced", recipeVoxels);
        int totalCut       = __state.WorkItem.Attributes.GetInt("mh_voxelsCut", 0);
        int scrappedVoxels = Math.Max(0, totalPlaced - recipeVoxels);

        __instance.Api.Logger.Debug(
            "[Mineholme] Scrap: placed={0} recipe={1} scrapped={2} cut={3}",
            totalPlaced, recipeVoxels, scrappedVoxels, totalCut);

        // 1 ingot = 100 units = 42 voxels = 20 nuggets. Each voxel = 2.3 units, each nugget = 5 units.
        int bits = (int)Math.Floor(totalCut * 2.3 / 5.0);
        if (bits <= 0) return;

        string? metal = __state.WorkItem.Collectible?.Variant["metal"];
        if (string.IsNullOrEmpty(metal)) return;

        Item? bitItem = __instance.Api.World.GetItem(new AssetLocation($"game:metalbit-{metal}"));
        if (bitItem == null) return;

        var bitStack = new ItemStack(bitItem, bits);
        float temp = __state.WorkItem.Collectible?.GetTemperature(__instance.Api.World, __state.WorkItem) ?? 0f;
        bitItem.SetTemperature(__instance.Api.World, bitStack, temp);

        __instance.Api.World.SpawnItemEntity(bitStack,
            __instance.Pos.ToVec3d().Add(0.5, 0.75, 0.5));
    }
}

// ── ItemIngot.TryPlaceOn postfix — track voxels placed ──────────────────────

[HarmonyPatch(typeof(ItemIngot), nameof(ItemIngot.TryPlaceOn))]
static class ItemIngotTryPlaceOnPatch
{
    static readonly Dictionary<int, long> _lastPlaceMs = new();

    [HarmonyPostfix]
    static void Postfix(ItemStack stack, BlockEntityAnvil beAnvil, ItemStack? __result)
    {
        if (__result == null) return;
        if (beAnvil.Api.World.Side != EnumAppSide.Server) return;

        int posKey = beAnvil.Pos.GetHashCode();
        long nowMs = beAnvil.Api.World.ElapsedMilliseconds;
        if (_lastPlaceMs.TryGetValue(posKey, out long lastMs) && nowMs - lastMs < 50)
            return;
        _lastPlaceMs[posKey] = nowMs;

        ItemStack target = beAnvil.WorkItemStack ?? __result;
        int prev = target.Attributes.GetInt("mh_voxelsPlaced", 0);
        target.Attributes.SetInt("mh_voxelsPlaced", prev + ItemIngot.VoxelCount);
    }
}

// ── ItemIronBloom.TryPlaceOn postfix — track voxels placed ──────────────────

[HarmonyPatch(typeof(ItemIronBloom), nameof(ItemIronBloom.TryPlaceOn))]
static class ItemIronBloomTryPlaceOnPatch
{
    static readonly Dictionary<int, long> _lastPlaceMs = new();

    [HarmonyPostfix]
    static void Postfix(ItemStack stack, BlockEntityAnvil beAnvil, ItemStack? __result)
    {
        if (__result == null) return;
        if (beAnvil.Api.World.Side != EnumAppSide.Server) return;

        int posKey = beAnvil.Pos.GetHashCode();
        long nowMs = beAnvil.Api.World.ElapsedMilliseconds;
        if (_lastPlaceMs.TryGetValue(posKey, out long lastMs) && nowMs - lastMs < 50)
            return;
        _lastPlaceMs[posKey] = nowMs;

        ItemStack target = beAnvil.WorkItemStack ?? __result;
        int prev = target.Attributes.GetInt("mh_voxelsPlaced", 0);
        target.Attributes.SetInt("mh_voxelsPlaced", prev + ItemIngot.VoxelCount);
    }
}

// ── BlockEntityAnvil.OnSplit prefix — track chisel cuts ─────────────────────
// Each chisel cut that removes a metal voxel increments mh_voxelsCut.

[HarmonyPatch(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.OnSplit))]
static class SmithingCutTrackPatch
{
    static readonly Dictionary<long, long> _lastCutMs = new();

    // Anvil grid is 16×6×16; pack as anvilPosHash * 10000 + (x*400 + y*20 + z), max offset 6115 < 10000.
    static long CutKey(BlockPos pos, Vec3i v) =>
        (long)(uint)pos.GetHashCode() * 10000L + v.X * 400 + v.Y * 20 + v.Z;

    [HarmonyPrefix]
    static void Prefix(BlockEntityAnvil __instance, Vec3i voxelPos)
    {
        if (__instance.WorkItemStack == null) return;
        if (__instance.Api.World.Side != EnumAppSide.Server) return;
        if (__instance.Voxels[voxelPos.X, voxelPos.Y, voxelPos.Z] != (byte)EnumVoxelMaterial.Metal) return;

        long key = CutKey(__instance.Pos, voxelPos);
        long nowMs = __instance.Api.World.ElapsedMilliseconds;
        if (_lastCutMs.TryGetValue(key, out long lastMs) && nowMs - lastMs < 50)
            return;
        _lastCutMs[key] = nowMs;

        int prev = __instance.WorkItemStack.Attributes.GetInt("mh_voxelsCut", 0);
        __instance.WorkItemStack.Attributes.SetInt("mh_voxelsCut", prev + 1);
    }
}
