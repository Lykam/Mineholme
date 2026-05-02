# Mineholme Tasks

Current milestone: **v0.3 — Smithing and Metallurgy**

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

---

## Done

- [x] SmithingScrapPatch — confirmed working (double-fire fix via 500ms dedup guard)
- [x] Cast iron material definition (`metal-castiron.json`, textures, anvil tier restriction)
- [x] Dwarven cookpot block + grid recipe (cast iron)
- [x] CaveConnectorSystem written (two-phase backbone + component flood-fill)
- [x] All v0.1 features
- [x] All v0.2 features (brew quality tiers / aging deferred)
