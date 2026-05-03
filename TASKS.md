# Mineholme Tasks

Current milestone: **v0.3 — Smithing and Metallurgy**

---

## Bugs

### B1. Anvil → bits on first hammer use
Right-clicking a freshly placed anvil while holding a hammer instantly converts it (or the hammer) to metal bits.
SmithingBitPatch or SmithingScrapPatch is triggering on the empty-anvil interaction.
- [ ] Reproduce: smith an anvil, place it, right-click with hammer in hand
- [ ] Identify which patch fires and why (log the call stack or add a guard for empty work item + no selection)
- [ ] Add guard so the salvage / scrap path only fires when a work item is already on the anvil

### B2. Soil mushroom planting — immediate reharvest / infinite loop
Planting a cave mushroom drop on soil places the planted variant and leaves mycelium, but the block
can be immediately harvested again (the planted block drops the item on break). No growth delay.
Also: planting fires on right-click only, so it accidentally triggers when the player is eating with
a mushroom in hand.
- [ ] Restrict placement to sneak+use (same as block placement), or require the active hand slot check
- [ ] Disallow immediate harvest: planted block on soil should require a delay (minimum 1 game day)
  before it drops anything, OR simply disable soil planting and only allow stone + farmland
- [ ] If soil planting is kept, ensure mycelium only spawns on stone (not soil)

### B3. Wall mushroom orientation — floating / bad placement (Deepbark etc.)
Wall mushrooms spawn floating or in wrong orientations because the worldgen block patch does not
guarantee a solid horizontal face exists. The `BlockCaveWallMushroom` class scans for a wall face
during `TryPlaceBlockForWorldGen` but placement still succeeds in open air.
- [ ] Audit `BlockCaveWallMushroom.TryPlaceBlockForWorldGen`: confirm it requires a solid adjacent face
      before placing (not just "checks and tries to orient")
- [ ] If the solid-face check is missing, add it — bail out and return false if no solid wall found
- [ ] Also verify the worldgen block patch offsets are correct so spawns target wall-adjacent positions

### B4. Mushroom worldgen — mixed varieties per cluster
Cave mushroom clusters spawn multiple types in the same patch instead of all being the same variety,
unlike vanilla above-ground mushrooms which produce uniform-type groups.
- [ ] Investigate how the worldgen `blocksByType` / block patch is structured in `cave-plants.json`
- [ ] Each patch entry should spawn only one type across the whole cluster; fix so that each entry in
  the block patches array is a single-type cluster (one entry per type, not a wildcard that randomly
  assigns per block)

---

## In Progress

---

## Up Next

### 1. Test SmithingBitPatch (bit adding + tool salvage)
- [ ] Heat a metalbit → shift-click anvil with active work item → 2 voxels added
- [ ] Cold bit rejected with error message
- [ ] Wrong-metal bit rejected
- [ ] Shift-click a finished metal tool onto anvil → consumed, bits spawned (~50% recovery)
- [ ] Non-metal items not consumed
- [ ] Work item on anvil not salvageable via tool-salvage path

### 2. Test SmithingHelveFixPatch
- [ ] Place iron bloom on anvil, run helve hammer → completes without "not enough metal" error
- [ ] Bloom picked up mid-work and re-placed also gets padded correctly

### 3. Test SmithingToolBreakPatch + ItemBrokenToolHead repair chain
- [ ] Break a pickaxe to 0 durability → broken-toolhead spawns
- [ ] Heat broken head → shift-place on anvil → partial voxel grid appears (~80% filled)
- [ ] Smith missing voxels → correct tool drops on completion
- [ ] Pick up broken head without finishing → head returns to inventory
- [ ] GetHeldItemName shows reasonable label

### 4. Test BlockMagmaForge
- [ ] Place magmaforge adjacent to lava → auto-ignites, no charcoal needed
- [ ] Forge stays hot as long as lava is adjacent
- [ ] Remove lava → forge cools (normal fuel consumption resumes)
- [ ] Place forge without lava → behaves as normal forge
- [ ] Place lava AFTER placing forge → forge lights (neighbor-change trigger)

### 5. Verify cast iron
- [ ] `ingot-castiron` exists in creative inventory
- [ ] Cast iron ingot can be smelted in crucible (meltingPoint 1200)
- [ ] Cast iron ingot cannot be placed on anvil (tier 99 blocks it)
- [ ] `metalbit-castiron` exists (auto-created via worldproperty, or needs a patch)
- [ ] Check logs for `worldproperties/block/metal` patch error — correct path may be `worldproperties/object/metal`

### 6. Test CaveConnectorSystem
- [ ] Generate a new world → check that isolated cave pockets have connecting tunnels
- [ ] Tunnels are organic (not axis-aligned bores)
- [ ] No crashes or worldgen errors in logs
- [ ] Check `/vs-logs` for patch errors

### 7. Expanded molds (cast iron)
- Define cast iron grating item + casting mold recipe
- Define cast iron lamp post item + casting mold recipe
- Define at least one hardware piece (hinge or bracket)
- Add lang keys and creative inventory entries

### 8. Magma forge crafting recipe
- Grid recipe: player can craft the magmaforge block from iron/cast iron components
- Add to `recipes/grid/smithing/`

### 9. Cave moss texture + logic pass
Current texture is a fern shape. Replace with a flat spreading moss that reads like the lichen/moss
that grows on tree bark in vanilla.
- [ ] Find a suitable vanilla texture or create a new one (look at `block/plant/lichen*` or `block/plant/moss*`)
- [ ] Update cave-moss blocktype shape/texture reference
- [ ] Review placement logic: should spread on stone ceilings and walls, not just floors
- [ ] Tune worldgen spawn rate after texture change so density still reads correctly

---

## Research

### R1. Stability system — underground living incentive
VS has a sanity / temporal stability system. Research how it works mechanically
(attributes, decay rates, structure radius bonuses) and design a Mineholme mechanic
that rewards players for living underground.
- [ ] Read `EntityBehaviorTemporalStabilityAffected` and related classes in VS source
- [ ] Understand what boosts stability (player-built structures, temporal gears, distance from rifts)
- [ ] Design proposal: underground structures provide a stability aura / deep shelter bonus
- [ ] Determine if this can be implemented via Harmony patch or a registered entity behavior

---

## Future / Post-v0.3

### F1. Handbook pass
After all v0.3 systems are final, do a full handbook review pass:
- Show mushroom buff groups and stat effects in item descriptions
- Add handbook entries for dwarven dishes explaining stat bonuses
- Add cave ale / scotch flavor text + effect summaries
- Ensure all mineholme blocks and items have `handbook: { include: true }` set
- Review handbook order (creativeinventory groups) so items appear logically grouped

---

## Done

- [x] SmithingScrapPatch — confirmed working (double-fire fix via 500ms dedup guard)
- [x] Cast iron material definition (`metal-castiron.json`, textures, anvil tier restriction)
- [x] Dwarven cookpot block + grid recipe (cast iron)
- [x] CaveConnectorSystem written (two-phase backbone + component flood-fill)
- [x] BetterRuins compatibility — `betterruins-castiron.json` patch skips castiron in corroded-sheet recipe
- [x] All v0.1 features
- [x] All v0.2 features (brew quality tiers / aging deferred)
