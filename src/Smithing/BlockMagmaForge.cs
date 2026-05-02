using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Mineholme;

/// <summary>
/// A dwarven forge that operates without fuel when an adjacent block is lava.
/// Extends vanilla BlockForge so all normal interactions (add coal, ignite,
/// place/take work items) still function.  When lava is adjacent the forge
/// is automatically stocked with charcoal and ignited, making it permanently
/// active for as long as lava remains adjacent.
/// </summary>
public class BlockMagmaForge : BlockForge
{
    // FuelLevel on BlockEntityForge is read-only (computed from FuelSlot.StackSize).
    // We top up by placing charcoal into the FuelSlot directly.
    // This field reflects into partialFuelConsumed so we can reset burn progress.
    static readonly FieldInfo? _partialFuelConsumed =
        AccessTools.Field(typeof(BlockEntityForge), "partialFuelConsumed");

    // ── Lava adjacency ────────────────────────────────────────────────────────

    public bool HasAdjacentLava(IWorldAccessor world, BlockPos pos)
    {
        foreach (BlockFacing face in BlockFacing.ALLFACES)
        {
            Block adj = world.BlockAccessor.GetBlock(pos.AddCopy(face));
            if (adj?.Code?.Path.Contains("lava") == true) return true;
        }
        return false;
    }

    // ── Convenience: fuel + ignite the forge when lava is present ─────────────

    private void TryLavaIgnite(IWorldAccessor world, BlockPos pos)
    {
        if (world.Side != EnumAppSide.Server) return;
        if (!HasAdjacentLava(world, pos)) return;

        BlockEntityForge? bef = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityForge;
        if (bef == null) return;

        // Top up the fuel slot with charcoal if it's low.
        // FuelLevel = FuelSlot.StackSize - partialFuelConsumed; target is ~10 units.
        const int targetFuel = 10;
        if (bef.FuelLevel < 4f)
        {
            // Reset partial consumption so the stored stack reads correctly.
            _partialFuelConsumed?.SetValue(bef, 0f);

            Item? charcoal = world.GetItem(new AssetLocation("game:charcoal"));
            if (charcoal != null && bef.FuelSlot.Itemstack == null)
            {
                bef.FuelSlot.Itemstack = new ItemStack(charcoal, targetFuel);
            }
            else if (charcoal != null && bef.FuelSlot.Itemstack?.Collectible == charcoal)
            {
                bef.FuelSlot.Itemstack.StackSize = targetFuel;
            }
        }

        if (!bef.IsBurning && bef.CanIgnite)
        {
            bef.TryIgnite();
        }

        bef.MarkDirty(true);
    }

    // ── Auto-stock on placement ────────────────────────────────────────────────

    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool placed = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        if (placed) TryLavaIgnite(world, blockSel.Position);
        return placed;
    }

    // ── React when a neighbour changes (lava flows in, placed beside forge) ────

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        base.OnNeighbourBlockChange(world, pos, neibpos);
        TryLavaIgnite(world, pos);
    }

    // ── Keep topped-up on each player interaction ──────────────────────────────

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        TryLavaIgnite(world, blockSel.Position);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    // ── Public query ──────────────────────────────────────────────────────────

    public bool IsLavaPowered(IWorldAccessor world, BlockPos pos)
        => HasAdjacentLava(world, pos);
}
