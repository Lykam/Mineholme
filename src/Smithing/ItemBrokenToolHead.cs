using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Mineholme;

/// <summary>
/// Represents a broken metal tool head that can be re-worked on an anvil.
/// The item carries the original tool's metal, code, and temperature as attributes.
/// </summary>
public class ItemBrokenToolHead : Item, IAnvilWorkable
{
    public int GetRequiredAnvilTier(ItemStack stack)
    {
        string? metal = stack.Attributes.GetString("broken_metal");
        if (string.IsNullOrEmpty(metal)) return 0;

        var core = api.ModLoader.GetModSystem<SurvivalCoreSystem>();
#pragma warning disable CS8600
        if (core?.metalsByCode != null &&
            core.metalsByCode.TryGetValue(metal, out MetalPropertyVariant mv))
        {
            return Math.Max(0, mv.Tier - 1);
        }
#pragma warning restore CS8600

        return 0;
    }

    public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
    {
        string? toolCode = stack.Attributes.GetString("broken_toolcode");
        if (string.IsNullOrEmpty(toolCode)) return new List<SmithingRecipe>();

        return api.GetSmithingRecipes()
            .Where(r => r.Output.ResolvedItemstack?.Collectible.Code.ToShortString() == toolCode)
            .ToList();
    }

    public bool CanWork(ItemStack stack)
    {
        float temp  = GetTemperature(api.World, stack);
        string? metal = stack.Attributes.GetString("broken_metal");
        if (string.IsNullOrEmpty(metal)) return temp > 400f;

        // Reference the ingot for melting-point data.
        Item? ingotItem = api.World.GetItem(new AssetLocation($"game:ingot-{metal}"));
        if (ingotItem == null) return temp > 400f;

        float meltPt = ingotItem.GetMeltingPoint(api.World, null, new DummySlot(new ItemStack(ingotItem)));
        return temp >= meltPt / 2f;
    }

    public ItemStack? TryPlaceOn(ItemStack stack, BlockEntityAnvil beAnvil)
    {
        if (!CanWork(stack)) return null;
        if (beAnvil.WorkItemStack != null) return null;

        string? metal = stack.Attributes.GetString("broken_metal");
        if (string.IsNullOrEmpty(metal)) return null;

        var recipes = GetMatchingRecipes(stack);
        if (recipes.Count == 0) return null;

        // Pick the first matching recipe (player can cycle if needed).
        SmithingRecipe recipe = recipes[0];

        // Build voxel pattern: full recipe shape with ~20 % of voxels randomly removed
        // (representing the damage/wear on the broken tool).
        beAnvil.Voxels = new byte[16, 6, 16];
        var rand = api.World.Rand;

        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < recipe.QuantityLayers && y < 6; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    if (recipe.Voxels[x, y, z] && rand.NextDouble() > 0.2)
                        beAnvil.Voxels[x, y, z] = (byte)EnumVoxelMaterial.Metal;
                }
            }
        }

        // The work item is of the same metal type.
        Item? workItem = api.World.GetItem(new AssetLocation($"game:workitem-{metal}"));
        if (workItem == null) return null;

        var workStack = new ItemStack(workItem);
        workItem.SetTemperature(api.World, workStack, GetTemperature(api.World, stack));
        beAnvil.SelectedRecipeId = recipe.RecipeId;

        // Track voxels placed for scrap calculation — count what we actually put down.
        int placed = 0;
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 6; y++)
                for (int z = 0; z < 16; z++)
                    if (beAnvil.Voxels[x, y, z] == (byte)EnumVoxelMaterial.Metal) placed++;

        workStack.Attributes.SetInt("mh_voxelsPlaced", placed);

        return workStack;
    }

    /// <summary>
    /// If the player removes the work without completing it, return the broken head back.
    /// </summary>
    public ItemStack GetBaseMaterial(ItemStack stack) => stack;

    public EnumHelveWorkableMode GetHelveWorkableMode(ItemStack stack, BlockEntityAnvil beAnvil)
        => EnumHelveWorkableMode.FullyWorkable;

    public int VoxelCountForHandbook(ItemStack stack) => ItemIngot.VoxelCount;

    public override string GetHeldItemName(ItemStack itemStack)
    {
        string metal   = itemStack.Attributes.GetString("broken_metal") ?? "iron";
        string toolCodeFull = itemStack.Attributes.GetString("broken_toolcode") ?? "";

        // toolcode is like "game:sword-iron" or "mineholme:dwarvenaxe-iron"
        // Extract the bare tool-type word (part before the first '-' in the path).
        string path     = toolCodeFull.Contains(':') ? toolCodeFull.Split(':')[1] : toolCodeFull;
        string toolType = path.Contains('-') ? path.Split('-')[0] : path;

        // Try a localised name; fall back to a generic label if the key is missing.
        string key = $"mineholme:broken-{toolType}-head-{metal}";
        if (Lang.HasTranslation(key)) return Lang.Get(key);
        return Lang.Get("mineholme:item-broken-toolhead");
    }
}
