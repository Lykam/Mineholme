# Mineholme

> *"The world below the surface is the world worth living in."*

Mineholme is a survival overhaul mod built around a single premise: **underground life should be worth choosing, not just endured.** It adds new food systems, a brewing and distillation pipeline, farmland mechanics, mob density tuning, lore-flavored world details, and a mushroom-based inebriation system — all themed around a lost dwarven civilization that made the deep its home.

Requires Vintage Story **1.22.0+**.

---

## Underground Farming

The further you dig, the better your crops grow.

- **Underground plots** (artificial light ≥ 10, sunlight ≤ 2): crops grow **25% faster** and yield **+50%** at harvest
- **Surface plots** (sunlight ≥ 13): crops grow **20% slower** and yield **30% less**

This is a deliberate trade-off. The surface is harsher to farm. Dig down, build a grow-room, commit to the underground.

---

## Cave Plants & Forageables

Three new wild plants spawn on stone floors and surfaces in the deep:

- **Cave Moss** — a dense, flat ground cover; mild food source
- **Cave Glowcap** — a bioluminescent fungal cluster; slightly better nutrition
- **Cave Root** — a pale root vegetable that clings to cave ceilings and walls

These are the foundation ingredients for dwarven cooking and brewing. They spawn in visible patches at cave depth.

---

## Cave Mushrooms

Forty-five cave mushroom varieties grow throughout the underground — one for every vanilla mushroom type, adapted to darkness and stone. They come in two growth forms:

- **Floor mushrooms** (34 types) — grow on flat stone surfaces in clusters
- **Wall mushrooms** (11 types) — anchor to solid rock faces, growing outward into open space

Each variety has a dwarven lore name replacing the surface common name. These are not the mushrooms the surface knows. A few examples:

| Lore Name | Properties |
|---|---|
| Forgegold | Safe — high satiety, mining buff |
| Vault King | Safe — high satiety, mining buff |
| Stonebread | Safe — reliable food source |
| Veinbleed | **Dangerous** — health drain, movement penalty |
| Deathcap | **Lethal** — rare spawn, severe damage |
| Ghostshelf | Psychedelic — temporal stability effects |

Dangerous types carry real health penalties when eaten raw. Brewing neutralizes most hazards and concentrates the benefits.

---

## Brewing Pipeline

Cave mushrooms feed a full three-stage brewing and distillation pipeline.

**Stage 1 — Cave Ale** (45 varieties)  
Barrel-brew any cave mushroom type (5 mushrooms + 1L water, 480h seal) to produce 0.2L of its corresponding cave ale. Each ale carries the properties of its source mushroom — satiety, buff potential, or hazard — at reduced intensity.

**Stage 2 — Cave Spirit** (45 varieties)  
Run cave ale through a boiler and condenser (ratio 0.3). The distillation process concentrates the active compounds. Spirits are potent — consume carefully.

**Stage 3 — Cave Scotch** (45 varieties)  
Barrel-age cave spirit with peat (1L spirit + 2 peat, 1440h seal) to produce 0.9L of scotch. Smoother, slower-acting, and the most refined output of the pipeline.

All 135 products have unique lore names — *Goldseam Ale*, *Deep Dream*, *Doombonnet Drop*, *Last Cup* — matching the mushroom they came from.

**Deepbrew** is also available: a simpler mushroom ale brewed from vanilla surface mushrooms for players not yet underground.

---

## Inebriation System

Mineholme replaces the vanilla intoxication camera sway with a custom stat-based inebriation system. Effects are tied to the mushroom type used in brewing, organized into three stat groups:

- **Mining mushrooms** — inebriation scales mining speed; sweet-spot peak gives a noticeable mining buff before penalty kicks in
- **Movement mushrooms** — inebriation scales movement speed; deep inebriation causes stability drain
- **Temporal stability mushrooms** — inebriation directly affects temporal stability

Each group has three tiers (fine / drunk / absolutely wrecked). The system decays at the same rate as vanilla hunger — drink steadily to stay in the sweet spot, drink too much and you'll feel it.

---

## Dwarven Cooking

Three new prepared dishes using cave ingredients:

| Dish | Recipe | Notes |
|---|---|---|
| **Stonebread** | Cave mushroom drop + flour ×2 | Dense underground bread |
| **Spore Hash** | Cave mushroom drop + cave root | Earthy protein dish |
| **Moss Broth** | Glowcap + cave moss ×3 | Light forager's soup |

---

## Surface Balance Changes

Mineholme nudges surface survival toward scarcity to make the underground trade-off meaningful:

- **Vegetables**: −35% satiety
- **Fruits**: −30% satiety

This affects raw and cooked forms. The surface isn't unlivable — it's just a less efficient way to eat.

---

## Mob Density

Deeper caves are denser and more dangerous:

| Mob | Vanilla | Mineholme |
|---|---|---|
| Deep Drifter | 4 max | 8 max |
| Tainted Drifter | 3 max | 6 max |
| Corrupt Drifter | 3 max | 5 max |
| Nightmare Drifter | 3 max | 4 max |
| Locust (population) | 3 max | 6 max |
| Locust (spawn chance) | baseline | ×2 |

The underground is a resource — it should feel like one worth fighting for.

---

## Dwarven Remnants

### Translocators
Static translocators are reskinned as **Ancient Dwarven Gates** or **Ruined Dwarven Gates**, with new handbook lore entries explaining them as remnants of a dwarven transit network. Functional behavior is unchanged.

### Decorative Blocks
Four dwarven construction blocks for players building underground structures:

- **Dwarven Brazier** (iron / brass) — ornamental light source; crafted from metal rods and a torch
- **Dwarven Barrel (Decorative)** — a sealed display barrel; no functional storage
- **Dwarven Carved Stone** (granite / basalt / white marble) — dressed stone with carved geometric detail
- **Dwarven Grating** — iron floor/wall grating for ventilation shafts and drainage channels

---

## Compatibility & Notes

- Code mod — requires a C# runtime (standard for VS mods)
- All balance changes are intentional and calibrated for a harder, underground-focused playthrough
- Compatible with most content mods; may interact with other satiety or inebriation overhauls

---

*Mineholme is in active development. Balance, content, and systems will evolve.*
