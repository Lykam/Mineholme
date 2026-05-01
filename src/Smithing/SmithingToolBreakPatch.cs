using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

// When a metal tool breaks on zero durability, spawn a broken tool head that can be
// re-worked on the anvil to recover the item (minus some material) without a full ingot.
//
// We use prefix+postfix state because after DestroyItem runs, itemSlot.Itemstack is null.
// The prefix captures everything we need before the item is destroyed.

[HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.DamageItem))]
static class SmithingToolBreakPatch
{
    internal sealed class BreakState
    {
        public string Metal      = "";
        public string ToolCode   = "";
        public float  Temp;
        public Vec3d  SpawnPos   = new Vec3d();
    }

    [HarmonyPrefix]
    static void Prefix(
        IWorldAccessor world,
        Entity byEntity,
        ItemSlot itemSlot,
        int amount,
        bool destroyOnZeroDurability,
        ref BreakState? __state)
    {
        __state = null;
        if (!destroyOnZeroDurability) return;
        if (world.Side != EnumAppSide.Server) return;
        if (itemSlot?.Itemstack == null) return;

        ItemStack stack = itemSlot.Itemstack;

        // Must have remaining durability that will hit zero with this hit.
        int remaining = stack.Collectible.GetRemainingDurability(stack);
        if (remaining - amount > 0) return;

        // Must be a metal item.
        string? metal = stack.Collectible.Variant["metal"];
        if (string.IsNullOrEmpty(metal)) return;

        // Must be a tool (has a tool type).
        EnumTool? tool = stack.Collectible.GetTool(itemSlot);
        if (tool == null) return;

        // Exclude work items and bloom — those are already mid-smithing.
        if (stack.Collectible is IAnvilWorkable) return;

        __state = new BreakState
        {
            Metal    = metal,
            ToolCode = stack.Collectible.Code.ToShortString(),
            Temp     = stack.Collectible.GetTemperature(world, stack),
            SpawnPos = byEntity.Pos.XYZ.Clone()
        };
    }

    [HarmonyPostfix]
    static void Postfix(IWorldAccessor world, Entity byEntity, BreakState? __state)
    {
        if (__state == null) return;

        Item? brokenHead = world.GetItem(new AssetLocation("mineholme:broken-toolhead"));
        if (brokenHead == null) return;

        var brokenStack = new ItemStack(brokenHead);
        brokenStack.Attributes.SetString("broken_metal",   __state.Metal);
        brokenStack.Attributes.SetString("broken_toolcode", __state.ToolCode);
        brokenStack.Attributes.SetFloat("broken_temp",     __state.Temp);
        brokenHead.SetTemperature(world, brokenStack, __state.Temp);

        world.SpawnItemEntity(brokenStack, __state.SpawnPos.Add(0, 0.25, 0));
    }
}
