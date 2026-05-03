using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Mineholme;

// Prevent metal bits from being placed on any forge (vanilla BlockForge or BlockMagmaForge).
// Bits should only be added to an anvil work item via the shift-click mechanic.
[HarmonyPatch(typeof(BlockForge), nameof(BlockForge.OnBlockInteractStart))]
static class ForgeNuggetPatch
{
    [HarmonyPrefix]
    static bool Prefix(IWorldAccessor world, IPlayer byPlayer)
    {
        if (world.Side != EnumAppSide.Server) return true;
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        return slot?.Itemstack?.Collectible.FirstCodePart() != "metalbit";
    }
}
