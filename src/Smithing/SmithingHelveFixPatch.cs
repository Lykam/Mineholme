using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Mineholme;

// When an iron bloom is placed on an anvil the random voxel generation can occasionally
// produce fewer than 42 metal voxels, which prevents the helve hammer from being able to
// reach a recipe that needs a full ingot worth of material.
// This postfix ensures the anvil always starts with at least 42 metal voxels after a bloom
// is placed, filling any deficit from the bottom of the ingot column.

[HarmonyPatch(typeof(ItemIronBloom), nameof(ItemIronBloom.TryPlaceOn))]
static class SmithingHelveFixPatch
{
    [HarmonyPostfix]
    static void Postfix(ItemStack stack, BlockEntityAnvil beAnvil, ItemStack? __result)
    {
        if (__result == null) return;

        // Count existing metal voxels.
        int metalCount = 0;
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 6; y++)
                for (int z = 0; z < 16; z++)
                    if (beAnvil.Voxels[x, y, z] == (byte)EnumVoxelMaterial.Metal)
                        metalCount++;

        if (metalCount >= ItemIngot.VoxelCount) return; // already sufficient

        int deficit = ItemIngot.VoxelCount - metalCount;

        // Fill ingot columns bottom-to-top until deficit is met.
        // Ingot region: x in [4..10], y in [0..5], z in [6..8], y=0 preferred.
        for (int y = 0; y < 6 && deficit > 0; y++)
        {
            for (int x = 4; x <= 10 && deficit > 0; x++)
            {
                for (int z = 6; z <= 8 && deficit > 0; z++)
                {
                    if (beAnvil.Voxels[x, y, z] == (byte)EnumVoxelMaterial.Empty)
                    {
                        beAnvil.Voxels[x, y, z] = (byte)EnumVoxelMaterial.Metal;
                        deficit--;
                    }
                }
            }
        }

        beAnvil.MarkDirty();
    }
}
