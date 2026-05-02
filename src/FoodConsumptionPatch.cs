using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace Mineholme;

// Patches food eating to apply a small stat-group buff when the player eats
// a stat-keyed dwarven dish (stonebread-{stat}, spore-hash-{stat}).
// Routes to the same mh_{stat}_f WatchedAttribute used by the inebriation
// system, but at 0.5 per serving — less than a full cave-ale session.

[HarmonyPatch(typeof(CollectibleObject), "tryEatStop")]
static class FoodConsumptionPatch
{
    [HarmonyPrefix]
    static void Prefix(ItemSlot slot, ref string __state)
    {
        __state = slot?.Itemstack?.Collectible?.Code?.Path ?? "";
    }

    [HarmonyPostfix]
    static void Postfix(float secondsUsed, EntityAgent byEntity, string __state)
    {
        if (secondsUsed < 1.45f) return;
        if (byEntity?.World is not IServerWorldAccessor) return;

        string? stat = null;
        if (__state.StartsWith("dwarven-dish-stonebread-"))
            stat = __state["dwarven-dish-stonebread-".Length..];
        else if (__state.StartsWith("dwarven-dish-spore-hash-"))
            stat = __state["dwarven-dish-spore-hash-".Length..];

        if (stat != "mine" && stat != "move" && stat != "stab") return;

        float current = byEntity.WatchedAttributes.GetFloat($"mh_{stat}_f");
        byEntity.WatchedAttributes.SetFloat($"mh_{stat}_f", Math.Min(1.1f, current + 0.5f));
    }
}
