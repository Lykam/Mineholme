using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

// Extends ItemNugget with IAnvilWorkable so metal bits can start new anvil work items,
// exactly like ingots but contributing only 2 voxels per bit instead of 42.
// Subsequent bits are added via SmithingBitPatch (the top-up path).
public class ItemMetalBit : ItemNugget, IAnvilWorkable
{
    public int GetRequiredAnvilTier(ItemStack stack)
    {
        string metal = stack.Collectible.Variant["metal"] ?? stack.Collectible.LastCodePart();
        var core = api.ModLoader.GetModSystem<SurvivalCoreSystem>();
#pragma warning disable CS8600
        if (core?.metalsByCode != null &&
            core.metalsByCode.TryGetValue(metal, out MetalPropertyVariant mv))
            return System.Math.Max(0, mv.Tier - 1);
#pragma warning restore CS8600
        return 0;
    }

    // Return all smithing recipes that accept an ingot of this metal — same set an ingot
    // would show, which includes our small bit-only recipes (arrowheads, nails).
    public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
    {
        string metal = stack.Collectible.Variant["metal"] ?? stack.Collectible.LastCodePart();
        Item? ingotItem = api.World.GetItem(new AssetLocation($"game:ingot-{metal}"));
        if (ingotItem == null) return new();

        var ingotStack = new ItemStack(ingotItem);
        return api.GetSmithingRecipes()
            .Where(r => r.Ingredient?.SatisfiesAsIngredient(ingotStack) == true)
            .OrderBy(r => r.Output.ResolvedItemstack?.Collectible.Code.ToString() ?? "")
            .ToList();
    }

    public bool CanWork(ItemStack stack)
    {
        float temp = GetTemperature(api.World, stack);
        float meltPt = GetMeltingPoint(api.World, null, new DummySlot(stack));
        return temp >= meltPt / 2f;
    }

    // Called by vanilla anvil code only when WorkItemStack == null (starting a new work item).
    // Top-up of an existing work item is handled by SmithingBitPatch.
    public ItemStack? TryPlaceOn(ItemStack stack, BlockEntityAnvil beAnvil)
    {
        if (!CanWork(stack)) return null;
        if (beAnvil.WorkItemStack != null) return null;

        string metal = stack.Collectible.Variant["metal"] ?? stack.Collectible.LastCodePart();

        // Place 2 voxels at the center of the ingot region — these positions land inside
        // the arrowhead and nail patterns and are harmlessly hammered off for larger recipes.
        beAnvil.Voxels = new byte[16, 6, 16];
        beAnvil.Voxels[7, 0, 7] = (byte)EnumVoxelMaterial.Metal;
        beAnvil.Voxels[8, 0, 7] = (byte)EnumVoxelMaterial.Metal;

        Item? workItem = api.World.GetItem(new AssetLocation($"game:workitem-{metal}"));
        if (workItem == null) return null;

        var workStack = new ItemStack(workItem);
        workItem.SetTemperature(api.World, workStack, GetTemperature(api.World, stack));
        workStack.Attributes.SetInt("mh_voxelsPlaced", 2);

        return workStack;
    }

    public ItemStack GetBaseMaterial(ItemStack stack) => stack;

    // Bits shouldn't auto-complete via helve hammer — the helve would over-drive tiny recipes.
    public EnumHelveWorkableMode GetHelveWorkableMode(ItemStack stack, BlockEntityAnvil beAnvil)
        => EnumHelveWorkableMode.NotWorkable;

    public int VoxelCountForHandbook(ItemStack stack) => 2;
}
