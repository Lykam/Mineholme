using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Mineholme;

// Prefix on BlockEntityAnvil.OnPlayerInteract handles two shift-click interactions:
//
//   1. Shift-click with a hot metal bit → add 2 voxels to the ongoing work item.
//   2. Shift-click with a finished metal tool → salvage it into metal bits (50 % recovery).
//
// Because all Harmony prefixes on the same method are composed, this single class handles
// both cases and falls through to the original method when neither applies.

[HarmonyPatch(typeof(BlockEntityAnvil), "OnPlayerInteract")]
static class SmithingBitPatch
{
    // RegenMeshAndSelectionBoxes is protected — cache via MethodInfo once, call via Invoke.
    static readonly System.Reflection.MethodInfo _regenMeshMethod =
        AccessTools.Method(typeof(BlockEntityAnvil), "RegenMeshAndSelectionBoxes");

    static void InvokeRegenMesh(BlockEntityAnvil instance)
        => _regenMeshMethod.Invoke(instance, null);

    [HarmonyPrefix]
    static bool Prefix(
        BlockEntityAnvil __instance,
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        ref bool __result)
    {
        if (world.Side != EnumAppSide.Server) return true;
        if (!byPlayer.Entity.Controls.ShiftKey) return true;

        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        ItemStack? stack = slot.Itemstack;
        if (stack == null) return true;

        string firstPart = stack.Collectible.FirstCodePart();

        // ── Case 1: metal bit placement ──────────────────────────────────────
        if (firstPart == "metalbit")
        {
            if (__instance.WorkItemStack == null) return true;

            string bitMetal  = stack.Collectible.LastCodePart();
            string? workMetal = __instance.WorkItemStack.Collectible.Variant["metal"];

            if (workMetal != bitMetal) return true;

            // Check temperature
            float bitTemp     = stack.Collectible.GetTemperature(world, stack);
            float meltingPoint = stack.Collectible.GetMeltingPoint(world, null, new DummySlot(stack));
            if (bitTemp < meltingPoint / 2f)
            {
                if (byPlayer is IServerPlayer sp2)
                    ((ICoreServerAPI)__instance.Api).SendIngameError(sp2, "mh_bitcold", "Must heat the bit first");
                return true;
            }

            int added = AddNVoxels(__instance, 2);
            if (added > 0)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                __instance.CheckIfFinished(byPlayer);
                InvokeRegenMesh(__instance);
                __instance.MarkDirty();
                __result = true;
                return false;
            }
            else
            {
                if (byPlayer is IServerPlayer sp)
                    ((ICoreServerAPI)__instance.Api).SendIngameError(sp, "mh_bitfull", "No space to add more metal");
                return true;
            }
        }

        // ── Case 2: tool salvage ──────────────────────────────────────────────
        string? toolMetal = stack.Collectible.Variant["metal"];
        EnumTool? tool    = stack.Collectible.GetTool(slot);

        bool isMetal    = !string.IsNullOrEmpty(toolMetal);
        bool isToolItem = tool != null;
        bool isWorkable = stack.Collectible is IAnvilWorkable;

        if (isMetal && isToolItem && !isWorkable)
        {
            int bits = ComputeSalvageBits(__instance.Api, stack);
            if (bits <= 0) return true;

            Item? bitItem = world.GetItem(new AssetLocation($"game:metalbit-{toolMetal}"));
            if (bitItem == null) return true;

            float temp     = stack.Collectible.GetTemperature(world, stack);
            var bitStack   = new ItemStack(bitItem, bits);
            bitItem.SetTemperature(world, bitStack, temp);

            world.SpawnItemEntity(bitStack, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));

            slot.TakeOut(1);
            slot.MarkDirty();

            __result = true;
            return false;
        }

        return true;
    }

    // Fill up to n empty slots in the 7×2×3 ingot region, bottom layer first.
    // Returns how many voxels were actually placed.
    internal static int AddNVoxels(BlockEntityAnvil beAnvil, int n)
    {
        int added = 0;
        // Ingot area: x in [4..10], y in [0..1], z in [6..8]
        for (int y = 0; y < 2 && added < n; y++)
        {
            for (int x = 4; x <= 10 && added < n; x++)
            {
                for (int z = 6; z <= 8 && added < n; z++)
                {
                    if (beAnvil.Voxels[x, y, z] == (byte)EnumVoxelMaterial.Empty)
                    {
                        beAnvil.Voxels[x, y, z] = (byte)EnumVoxelMaterial.Metal;
                        added++;
                    }
                }
            }
        }
        return added;
    }

    // Compute how many bits a tool yields at 50 % recipe-voxel recovery.
    private static int ComputeSalvageBits(ICoreAPI api, ItemStack toolStack)
    {
        string toolShortCode = toolStack.Collectible.Code.ToShortString();

        SmithingRecipe? recipe = api.GetSmithingRecipes()
            .FirstOrDefault(r => r.Output.ResolvedItemstack?.Collectible.Code.ToShortString() == toolShortCode);

        int recipeVoxels;
        if (recipe != null)
        {
            recipeVoxels = 0;
            bool[,,] rv  = recipe.Voxels;
            int lx = rv.GetLength(0), ly = rv.GetLength(1), lz = rv.GetLength(2);
            for (int x = 0; x < lx; x++)
                for (int y = 0; y < ly; y++)
                    for (int z = 0; z < lz; z++)
                        if (rv[x, y, z]) recipeVoxels++;
        }
        else
        {
            // Fallback: estimate from max durability
            int maxDura = toolStack.Collectible.GetMaxDurability(toolStack);
            recipeVoxels = Math.Max(21, maxDura / 50 * 2); // rough inverse of durability
        }

        // 50 % recovery, 2.1 voxels per bit
        return Math.Max(1, (int)Math.Floor(recipeVoxels * 0.5 / 2.1));
    }
}
