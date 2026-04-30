using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Mineholme;

// Patches both consumption code paths: direct items (CollectibleObject.tryEatStop)
// and liquid containers (BlockLiquidContainerBase.tryEatStop).
// Each pair captures the vanilla intox delta per sip and routes it to the right
// WatchedAttribute for our buff system.

[HarmonyPatch(typeof(CollectibleObject), "tryEatStop")]
static class CollectibleObjectDrinkPatch
{
    [HarmonyPrefix]
    static void Prefix(EntityAgent byEntity, ref float __state)
    {
        __state = byEntity?.WatchedAttributes?.GetFloat("intoxication") ?? 0f;
    }

    [HarmonyPostfix]
    static void Postfix(float secondsUsed, ItemSlot slot, EntityAgent byEntity, float __state)
    {
        if (byEntity?.World is not IServerWorldAccessor) return;
        float delta = byEntity.WatchedAttributes.GetFloat("intoxication") - __state;
        if (delta <= 0f) return;
        DrinkTierRouter.Route(byEntity, slot.Itemstack, delta);
    }
}

[HarmonyPatch(typeof(BlockLiquidContainerBase), "tryEatStop")]
static class LiquidContainerDrinkPatch
{
    [HarmonyPrefix]
    static void Prefix(EntityAgent byEntity, ref float __state)
    {
        __state = byEntity?.WatchedAttributes?.GetFloat("intoxication") ?? 0f;
    }

    [HarmonyPostfix]
    static void Postfix(float secondsUsed, ItemSlot slot, EntityAgent byEntity, float __state,
        BlockLiquidContainerBase __instance)
    {
        if (byEntity?.World is not IServerWorldAccessor) return;
        float delta = byEntity.WatchedAttributes.GetFloat("intoxication") - __state;
        if (delta <= 0f) return;
        ItemStack? liquid = slot.Itemstack != null ? __instance.GetContent(slot.Itemstack) : null;
        DrinkTierRouter.Route(byEntity, liquid, delta);
    }
}

static class DrinkTierRouter
{
    // Cave mushroom drinks are detected by item code. The mushroom type is
    // extracted from the suffix and looked up in MushroomBuffGroups to pick
    // the stat attribute ("mine", "move", or "stab"). Tier suffix ("_f/d/a")
    // comes from which drink type (ale/spirit/scotch).
    //
    // All other alcoholic drinks (vanilla + mushroomaleportion) route to the
    // generic mh_intox_* attributes, which only remove the sober penalty.
    internal static void Route(Entity entity, ItemStack? stack, float delta)
    {
        var code = stack?.Collectible?.Code;
        if (code == null) return;

        if (code.Domain == "mineholme")
        {
            string? tierSuffix = null;
            string? mushroomType = null;

            if (code.Path.StartsWith("cave-ale-"))
            {
                tierSuffix = "_f";
                mushroomType = code.Path["cave-ale-".Length..];
            }
            else if (code.Path.StartsWith("cave-spirit-"))
            {
                tierSuffix = "_d";
                mushroomType = code.Path["cave-spirit-".Length..];
            }
            else if (code.Path.StartsWith("cave-scotch-"))
            {
                tierSuffix = "_a";
                mushroomType = code.Path["cave-scotch-".Length..];
            }

            if (tierSuffix != null && mushroomType != null)
            {
                string? stat = MushroomBuffGroups.GetStat(mushroomType);
                if (stat != null)
                {
                    AddToAttr(entity, $"mh_{stat}{tierSuffix}", delta);
                    return;
                }
            }
        }

        // Generic path: vanilla drinks + mushroomaleportion
        int tier = stack?.Collectible?.Attributes?["mineholme_inebriation_tier"].AsInt(0) ?? 0;
        if (tier < 1 || tier > 3) return;
        string genericKey = tier == 1 ? "mh_intox_f" : tier == 2 ? "mh_intox_d" : "mh_intox_a";
        AddToAttr(entity, genericKey, delta);
    }

    static void AddToAttr(Entity entity, string key, float delta)
    {
        float current = entity.WatchedAttributes.GetFloat(key);
        entity.WatchedAttributes.SetFloat(key, Math.Min(1.1f, current + delta));
    }
}
