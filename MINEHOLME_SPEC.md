# Mineholme

*A Vintage Story mod that makes the world below the surface the world worth living in.*

---

## What Mineholme Is

Mineholme is a Vintage Story mod inspired by Dwarf Fortress. It transforms
the game's underground from a place you visit for ore into a place you live.
Surface farming becomes a thin existence; the deeps become rich, dangerous,
and rewarding.

The mod ships as a single content-and-mechanics package — no required
companion mods, no dependencies. It's designed for any number of players
on a server, from solo to large communities, with all systems built
multiplayer-first.

Players choose one of three underground races at character creation —
Dwarf, Gnome, or Drow — and play through a world where mushrooms beat
vegetables for nutrition, ale beats water for sustenance, smithing rewards
care and recovers your mistakes, and the deepest places hold both the best
treasures and the worst threats.

## Design Pillars

Five principles that govern every design decision in this mod. When a
feature conflicts with a pillar, the pillar wins.

**1. Underground is home.**
The mod inverts Vintage Story's default play loop. Surface life is harder
and less rewarding than living underground. Food, drink, ores, ambiance,
and progression all push the player downward.

**2. Drink is necessary, not decoration.**
Ale, mead, wine, and spirits are not flavor items. They provide the buffs
that make a working day in the mines possible. A sober dwarf is an
underperforming dwarf. Brewing infrastructure is mandatory, not optional.

**3. Smithing is forgiving and deep.**
Mistakes recover. Broken tools repair. Bits and rods reclaim. Cast iron
and expanded molds reward investment in proper smithy infrastructure.
The system rewards craft over churn.

**4. Race shapes play.**
Choosing Dwarf, Gnome, or Drow changes how a player eats, drinks, fights,
mines, and trades. Mixed-race parties have natural division of labor.
The race system is data-driven from day one so the community can extend it.

**5. Multiplayer-first.**
Every mechanic works for any number of players. The world has a shared
history; players have personal histories. Discoveries reward the finder
without locking out latecomers. No system assumes a single player.

## Core Themes from Dwarf Fortress

Mineholme draws from DF deliberately, but adapts rather than imitates.
What the mod takes:

- **Ambition rewarded, hubris punished.** Digging deep enough finds wealth
  *and* trouble. Glittering halls attract attention from things that should
  not be disturbed.
- **Everything has history.** Items remember who made them and when. The
  world advances through ages. Players leave behind gravestones, engravings,
  and named masterworks that persist after they log off — or die.
- **Specialization through race.** Different races excel at different work.
  A complete fortress wants more than one kind of hands.
- **Drink as infrastructure.** Brewing isn't a side hobby; it's a load-
  bearing pillar of fortress operation.
- **Memorable failure.** "Losing is fun" — death has weight, with epitaphs
  and persistent gravestones, but doesn't erase the world's story.

What the mod deliberately does not take:

- Body-part-level combat (incompatible with VS's combat system)
- Mood-driven NPC simulation (no NPCs to simulate)
- Adventure mode / fortress mode duality (one mode, the player's mode)
- Caravan economy depth (VS has its own trader system, sufficient as-is)

## Player Experience: A Day in Mineholme

A new player spawns. A character creation prompt asks: Dwarf, Gnome, or
Drow? The player picks Dwarf. The world begins.

The early game is recognizable Vintage Story — the player gathers reeds,
makes a fire, finds shelter. But surface foraging feels lean. Vegetables
fill the satiety bar less than they should. Water quenches thirst but
provides nothing else. The first time the player finds a mushroom, it
restores more than expected. The world is signaling.

The player descends. A copper vein, then iron — but only in caves now,
not at the surface. Mining is faster than it used to be (the Dwarf trait).
The player builds a small forge near a cave entrance.

Crafting an iron pickaxe, a roll comes up: this one's special. A modal
appears: name your masterwork, or pick from the list. The player chooses
"Stoneheart's Bite." The pickaxe gets a generated description: *"On the
head is a depiction of a drifter being slain. The drifter is striking a
defiant pose."* The pickaxe will outlast normal tools and mine faster.

Hours later, the player has built a small underground hall. They've
brewed a barrel of ale. They drink. The mining-speed buff kicks in.
They notice that without the ale, when sobriety creeps back in, mining
slows down. Drinking isn't optional anymore.

A friend joins the server. The friend rolls Gnome. Where the Dwarf was
strong on raw mining, the Gnome's crossbow drops drifters from a distance
and her temporal stability lets her work the deeper tiers. Together they
explore farther than either could alone.

A third player joins, picks Drow. The party can no longer easily visit
the trader caravan — the Drow gets refused, stones are thrown — but in
combat the Drow's stealth-strike makes short work of the cave drifters
that have been giving them trouble. The party adjusts: the Dwarf and
Gnome handle trade runs, the Drow handles dangerous scouting.

Months later (game-time), the party stumbles on something genuinely big:
a carved gate at the back of a deep tunnel. Beyond it lies an abandoned
dwarven hall — chambers, workshops, a ruined throne, descending levels
of increasing risk and reward. Cracked vessels everywhere. The party
spends weeks excavating it. Each player finds different loot from different
vessels. The vault at the bottom has a single masterwork dwarven hammer —
the first player to claim it gets it. The others find recipes and rare
ingots. Everyone leaves with a story.

Eventually the dwarf player dies. A gravestone appears at the death site
with a generated epitaph: *"Here lies Jeff Stonehammer, slain by a forgotten
beast at depth -127, year 4 of the Iron Age."* The world remembers.

## What Ships in Mineholme

The mod is large in scope. Rather than describe everything in one block,
this section organizes features by the system they belong to. Each system
has a goal (what experience it creates), key behaviors (what the player
sees and does), and acceptance criteria (how we know it's working).

---

### Food and Sustenance

**Goal.** Make underground food sources nutritionally superior to surface
ones, so the player's diet naturally pulls them into caves.

**Key behaviors.**
- Mushrooms (king bolete, shiitake, glowcap, cultivated cave varieties) are
  the most filling food in the game.
- Surface vegetables and fruits provide reduced satiety compared to vanilla.
- Grains stay middling — they require processing anyway, so they remain a
  reasonable food source.
- A new line of cave-only edible plants (cave moss, glowcap, root-vegetables-
  on-cave-walls) gives players foraging targets in dark places.
- New cultivated mushroom blocks let the player farm fungi underground in
  the right conditions (low light, suitable substrate).
- Race-specific nutrition multipliers: Dwarves digest meat and ale better,
  Drow are carnivorous-leaning, Gnomes prefer grains.

**Acceptance criteria.**
- A surface-only farmer feels resource-starved compared to a mushroom-
  cultivating cave dweller.
- Mushroom satiety is materially higher than surface vegetable satiety.
- Players can sustain themselves indefinitely without ever tilling a surface
  field.

---

### Drink and the Inebriation System

**Goal.** Make alcohol mechanically necessary for productive underground
work. The thematic inversion: drunk is the baseline working state for an
underground civilization; sober is the impaired state.

**Key behaviors.**
- The mod uses Vintage Story's existing intoxication value as the buff
  scalar. Drinking applies vanilla intoxication; the mod hooks effects
  off it.
- Two natural alcohol tiers (matching vanilla):
  - *Fermented* (mead/ale/wine) — light buffs, short duration. The
    cheap maintenance drink. Keep a barrel, top off through the day.
  - *Distilled* (whiskey/vodka/brandy) — strong buffs, medium duration.
    The expedition drink. One swig before a deep run.
  - *Aqua vitae* — peak buffs, long duration, but easy to overshoot
    into vanilla's drunken-impairment penalties. The deliberate gamble.
- Positive effects scale with current intoxication: mining speed, temporal
  stability regen, minor combat resistance, expanded satiety cap.
- Sober penalties scale inversely: as intoxication drops below a threshold,
  mining slows, temporal stability decays faster, a subtle visual effect
  warns the player.
- Penalties scale with depth — being sober deep underground is genuinely
  punishing.
- The "sweet spot" is roughly 0.6–0.9 intoxication (just below vanilla's
  drunken-penalty cap of 1.1).
- Specialty named brews (Stoneblood Ale, Deepfire Whiskey, Tunnelrat Stout)
  layer secondary buffs on top of the inebriation effects: ore drop chance,
  fire resistance, night vision, and others.
- Quality tiers (Rough → Fair → Fine → Masterwork) determined by brewer
  skill and recipe variant.
- Aging system: barrels improve drink quality over in-game weeks. A
  Masterwork brew aged for a year is the apex.
- Race interactions: Dwarves hold their liquor (slower decay, higher peaks),
  Gnomes have slower tolerance buildup, Drow get a unique precision buff
  from distilled spirits.

**Acceptance criteria.**
- A player who never brews struggles to mine effectively at depth.
- The "sweet spot" is reachable through normal play without tedium.
- Different drinks feel meaningfully different to use.
- Brewing infrastructure becomes a recognizable part of every fortress.

**Reference.** SlowTox (Vintage Story mod) implements positive-effects
intoxication with tolerance buildup; useful study for proven balance numbers.

---

### Smithing and Metallurgy

**Goal.** Reward investment in proper smithing infrastructure. Mistakes
should recover; broken tools should repair; the smith who builds a real
forge should outpace the smith who improvises.

**Key behaviors.**
- Voxel scraps lost during smithing return as recoverable bits at work
  completion. Configurable percentage.
- Heated metal bits and short rods can be combined back into ingots or
  added to existing work items.
- Broken tools drop a "tool head" item that can be repaired at the anvil
  with bits, restoring most (but not all) durability.
- Cold crucibles can be scraped to recover stuck metal.
- New molds expand the casting catalog significantly:
  - Cast iron cookware (pots, pans, kettles)
  - Decorative blocks (gratings, lamp posts, hinges)
  - Specialty tools and hardware
- Cast iron as a new metal tier: castable, brittle, NOT smithable into
  weapons. Used for cookware, decorative blocks, and stoves.
- Magma forge: a forge variant placed adjacent to a lava source that
  smelts faster, hotter, and consumes no fuel. Lore: dwarves harness the
  heart of the world.
- Helve hammer fixes (iron blooms always have enough voxels for a full
  ingot under the helve hammer).
- Anvil-based salvage: place a finished item on the anvil, hammer it apart
  for bits.

**Acceptance criteria.**
- A player can smith for hours and feel that their material isn't being
  wasted.
- Broken tools feel like a recoverable problem, not a disposable loss.
- The casting system has enough variety to support decorative building.
- Cast iron has a clear gameplay role distinct from steel.

**References.** Smithing Plus, Thrifty Smithing, and Salvage+ (all Vintage
Story mods) demonstrate proven implementations of these mechanics.

---

### Races and Identity

**Goal.** Race choice should change how a player plays, not just what
their stat sheet says. Mixed-race parties should have natural division
of labor. The framework should be extensible enough that the community
can add new races.

**Three races ship at launch.**

**Dwarf.** The protagonist race; the system's balance baseline. Industrious,
ale-fueled, strong at mining and combat with heavy weapons, comfortable in
the deepest tiers, slower than other races on the surface. The "main
character" race that everything else is calibrated against.

**Gnome.** The cerebral counterpoint. Cleverer than dwarves but physically
weaker. Better with crossbows and bows, exceptional temporal stability,
talented at prospecting. Most comfortable at shallow-to-mid depths;
uncomfortable in the very deepest tiers (gnomes are not at home in the
truly far down). Gnomish recipes lean toward mechanism and instrument.

**Drow.** The adversarial raider archetype. Stealth-focused, light-melee
and crossbow specialists. Regenerate health in deep darkness; suffer real
penalties in bright light. Carnivorous-leaning diet. Severe surface
penalty. Trader-hostile by default — many traders refuse them or charge
punitive prices, with server-config options ranging from mild caution to
attack-on-sight. Drow get unique dialogue paths (intimidation, coercion)
that other races don't see, and unique recipes around poison, alchemy,
shadow-cloth, and light-dampening materials.

**Out of scope at launch.** Elves, Humans, Halflings, Goblins, Kobolds,
Duergar, Myconids, Deep Gnomes, Mole-folk. The framework supports these
as community addons or future official additions; they're a deliberate
omission, not an oversight.

**Race system design.**
- Race definitions are JSON files. Adding a new race should be possible
  without C# changes in the common case.
- Each race declares: identifier, display name, model overrides, base
  stats, diet multipliers, inebriation curve overrides, recipe gates,
  trader dialogue keys, and depth/light comfort range.
- Race is chosen at first character creation and is permanent (admin
  command and rare in-game item ("Mirror of Becoming") allow legitimate
  changes).
- Every race has a comfort range (Y-level brackets) and light tolerance
  range. Outside it, players accumulate penalties — stamina drain, mining
  speed reduction, accelerated temporal decay. This is a shared framework
  every race plugs into.
- Some recipes are race-gated. Recipe scrolls found in dwarven kingdoms
  can teach cross-race recipes occasionally.
- Trader interactions vary by race pairing. Each player's interaction is
  independent — traders don't carry state from previous players.

**Acceptance criteria.**
- A player can describe what their race is good at after one play session.
- Mixed-race parties find natural role specialization without explicit
  instruction.
- A community modder can add a new race by writing one JSON file.
- Drow players have a meaningfully different play experience from
  Dwarf or Gnome players.

**References.** Extra Race Traits and Racial Equality (Vintage Story
mods) demonstrate proven race-trait systems; PlayerModelLib provides
proven patterns for race attribute storage.

---

### Worldgen and Caves

**Goal.** Make underground spaces worth living in. Caves should be visually
varied, atmospheric, and large enough to build in. Surface ores should
disappear so descent is required for progression.

**Key behaviors.**
- Ore generation is suppressed at the surface. Most ores spawn only in
  caves; deeper Y-levels host higher-tier ores.
- New deep-tier ores (including an Adamantine analogue forming long
  vertical veins, gemstone density increasing with depth, and an optional
  endgame metal tier above steel).
- Increased cave density and depth, with vertical reach.
- Underground biomes: lava caves (hot, hostile), crystal caves (gem-rich,
  dangerous), fungal caverns (food-rich, peaceful), drowned caves (water-
  filled). Each has a distinct block palette, mob ecology, and ambient
  flavor.
- Cave-only ambient critters: bats, cave rats, blind fish.
- Underground vista features (the "caves are boring" problem):
  - Massive chambers (occasional oversized voids — atria, columned halls
    with stalactite forests).
  - Underground waterfalls and pools where surface streams meet caves.
  - Light shafts where caves come close to the surface.
  - Crystal formations that emit faint colored light.
  - Bioluminescent flora — glowcaps, glowing moss, luminescent lichen.
- Connected cave networks: a worldgen post-process links isolated cave
  volumes with narrow tunnels, making sustained underground travel possible.
- (Stretch goal) Underground rivers — true flowing watercourses through
  cave systems, modeled on terrain-aware hydrology.

**Acceptance criteria.**
- A player who descends into the average cave finds something visually
  interesting within 50 blocks.
- Players can travel meaningful distances underground without surfacing.
- Different cave biomes feel distinct, not reskinned.
- Surface mining yields little; cave mining is the path forward.

**References.** All Caves Are Connected and Cave-Only Ore Spawns (Vintage
Story mods) for cave network and ore distribution; Algernon's Watersheds
for terrain-aware water flow modeling.

---

### Lost Dwarven Kingdoms

**Goal.** The headline exploration feature. A player who finds one of
these should remember it for the lifetime of their character. The
discovery is a story, not a checkbox.

**Key behaviors.**
- Massive abandoned multi-chamber complexes generated at worldgen, spanning
  hundreds of blocks horizontally and dozens vertically.
- Rare enough to feel special; common enough that most servers will have
  multiple. Density is server-config-tunable.
- Three layout archetypes:
  - **The Mountain Hall** — entrance gate at a cliff face, descends through
    ceremonial entrance, guard barracks, market chambers, the great hall,
    living quarters, workshops and forges, deep mines, and the vault.
  - **The Sunken Hold** — built into a chasm. Vertical, levels descending
    around a central shaft, bridges across gaps (many collapsed).
  - **The Buried Kingdom** — entered through a collapse exposing the hall
    far below the surface. Sprawls horizontally rather than vertically.
- Each complex has tiered risk/reward zones:
  - **Outer halls** — relatively intact, low-tier mobs, common loot.
  - **Living quarters and workshops** — tougher mobs, better loot, recipes.
  - **Great hall and vaults** — elite mobs, masterwork weapons, unique
    recipes, ancient ales (which interact with the aging system to provide
    massive Inebriation buffs), and forgotten beasts.
  - **The deeps (some complexes only)** — endgame content. A shaft going
    further down than makes geological sense.
- Loot philosophy: the *real* reward is unique recipes and blueprints,
  not raw items. Finding a kingdom permanently expands the player's
  capabilities. Each complex has 1-2 unique items that exist nowhere
  else in the world.
- Engravings (see Identity & Legacy) tell the story of why each kingdom
  fell. A player who reads them learns lore.

**Multiplayer fairness.**
- "Hero loot" — unique recipes, masterwork named items, the artifact in
  the deepest vault — is shared/world-instanced. One per kingdom; first
  finder wins. Preserves rarity and the prestige of discovery.
- "Scrap loot" — common materials, ales, repair components, faded
  engravings — uses cracked vessels (already a per-instance loot mechanism
  in vanilla) and per-player loot chests. Late arrivals still get rewarded
  for exploring without breaking the rare-loot economy.
- The hybrid model means a party of five exploring the same Mountain Hall
  each gets meaningful rewards, but the legendary items remain legendary.

**Acceptance criteria.**
- Players talk about the kingdom they found — they remember it.
- A late-arriving party member who explores a kingdom an hour after a
  friend cleared it still finds rewarding loot.
- Hero items remain rare on a server even after multiple kingdoms have
  been found.
- Each archetype feels distinct; players can describe the differences.

**References.** Better Ruins (open-source Vintage Story mod) is the
technical model for multi-chunk structures; LOOTR (Minecraft mod) is the
conceptual model for per-player loot.

---

### Identity and Legacy

**Goal.** The world should remember. Items, places, and players should
accumulate history that persists. Players should leave traces.

**Key behaviors.**

*Masterwork crafting.* Rare procs during crafting produce a unique,
named, superior-stat item. The proc rate is low (1–2% base) and modified
by player race, depth, the quality of the smithy/brewery being used, and
inebriation magnitude (a slightly drunk dwarf is a slightly more inspired
craftsman). On proc, a modal offers two paths: name the item yourself,
or pick from a procedurally generated list. The item gets a generated
description-engraving ("On the head is a depiction of...") drawing from
either real player events or a stock pool. Attribution ("Made by [Player]
in year [N] of the [Age]") is permanent on the item. This is the single
most memorable feature in the mod.

*Procedural names.* Generated from component pools — adjective + noun +
optional epithet. "Stoneheart's Bite," "Coalflame, Bane of Drifters,"
"The Echoing Hammer." Hundreds of components yield tens of thousands of
possible names. Easily extensible via JSON.

*Engravings.* Players can engrave stone blocks with custom messages using
chisels and parchment. Engravings persist as part of the world. Specialty
"memorial slab" blocks support formatted plaques.

*Player event log.* Per-player history tracker: first ore tier mined,
first kill of each mob type, deaths, year transitions, deepest Y reached.
Stored as player attributes, used by masterwork engravings and gravestone
epitaphs. A `/legend` command lets the player read their own history.

*Gravestones.* On player death, an auto-generated gravestone block appears
at the death site with an epitaph drawn from the player's event log. "Here
lies Jeff Stonehammer, slain by a forgotten beast at depth -127, year 4
of the Iron Age." Server-wide registry; `/findgrave [name]` locates
fallen comrades.

*World ages.* The world advances through Stone Age → Copper Age → Bronze
Age → Iron Age → Steel Age → Deep Age based on the *server's* collective
tech progression — whoever first hits a milestone advances everyone.
Used in masterwork attributions and gravestones; mostly cosmetic but
provides narrative scaffolding.

**Acceptance criteria.**
- A player remembers the name of their first masterwork.
- An engraved stone in a fortress is still readable a year later.
- Gravestones build up over time, especially on long-running servers,
  visibly recording the world's casualties.
- A new player joining a long-running server can read evidence of the
  history that came before them.

---

### Mobs and Underground Threats

**Goal.** Make caves feel inhabited, increasingly dangerous with depth,
and capable of generating memorable encounters.

**Key behaviors.**
- Existing hostile entities (drifters, locusts, wolves) get expanded
  underground spawn rules — deeper, more varied, more frequent in the
  deeps.
- Cave-only mob variants: deep drifters (faster, more temporal damage),
  cave wolves (pack hunters), tunnel locusts (burrowers).
- Forgotten Beasts: rare, deep-spawning unique mini-bosses. Each has a
  procedurally generated stat block (tough, fast, fire-breathing, poisonous,
  etc.) and a unique generated name. Drops unique loot. Server-wide
  registry tracks named beasts that have spawned and what slew them.
- Visible wealth (decorative blocks, precious metals, gem inlays) in
  player-built spaces increases the spawn weight of forgotten beasts and
  other deep threats nearby. The lore: *the deep things hear the sound of
  gold and ale-songs*. The mechanic: a glittering hall draws attention.

**Acceptance criteria.**
- Cave depth correlates with mob threat — players feel real fear at depth.
- A forgotten beast appearance is a memorable event, not a routine
  encounter.
- A wealthy fortress feels riskier than a humble outpost.

---

### Atmosphere and Environmental Detail

**Goal.** Caves should feel like places, not just dungeon corridors.
Sound, light, air, and ambient detail should establish that you're in
something living, not a boring tunnel.

**Key behaviors.**
- Custom deep-cave audio: distant rumbles, dripping, occasional unsettling
  noises, biome-specific ambient layers (lava cracks, fungal hums, water
  flow). Plays alongside vanilla cave audio.
- Bioluminescent flora provides natural soft cave lighting — navigating
  the deeps shouldn't require constant torch placement to feel alive.
- Vermin and pests: cave rats spawn near unprotected food storage and
  consume it over time. Cats hunt vermin. Sealed storage blocks prevent
  infestation. Adds a small ecosystem layer to fortress maintenance.
- Caged creatures / menagerie: a new trap block catches small wildlife
  alive, and a cage block displays them in your hall. Some rare creatures
  only spawn in deep biomes — completing a menagerie is a side goal.
  Ties to the visible-wealth system (an extensive menagerie counts as
  wealth).

**Optional / extreme atmosphere features (configurable):**
- Cave-ins / structural integrity. Unsupported overhangs and large spans
  collapse over time, forcing pillar-and-stall mining. Configurable
  severity; off by default to avoid griefing on multiplayer servers.
- Dwarven king's missive: rare event where a translocator dumps out a
  sealed letter demanding a specific craft within a deadline. Completion
  grants a rare reward. A coop-friendly analog to DF's noble demands.

**Acceptance criteria.**
- A player walking through a cave for the first time describes it as
  atmospheric, not generic.
- Different cave biomes sound and look meaningfully different.
- The fortress feels alive — not just walls and chests.

---

### Building and Architecture

**Goal.** Give players the visual vocabulary to build a *dwarven fortress*,
not just a generic stone box.

**Key behaviors.**
- A new decorative block palette: carved stone variants, dwarven runic
  blocks, brass braziers, ale barrels (decorative), mushroom planters,
  cast iron grating and lamp posts.
- Mead hall furniture: long tables, benches, thrones, hearths.
- Improved underground lighting options: glowcaps, wall-mounted oil lamps
  with longer burn times, crystal lamps from gem combinations.
- Architectural utility blocks for big projects: dwarven-scale doors,
  fortress-scale archways.

**Acceptance criteria.**
- A player can build a recognizably "dwarven" hall using only mod blocks.
- The decorative palette has enough variety that two fortresses don't
  look identical.

---

### Combat and Tools

**Goal.** Race and environment should both shape combat. A dwarf with a
hammer in a tunnel should feel different from a drow with a crossbow in
the deep dark.

**Key behaviors.**
- Dwarven weapon tier: war hammers, picks-as-weapons, improved crossbows
  with bigger magazines and better ammo.
- Mining-as-combat: picks deal extra damage to cave-spawn mobs. Lore: the
  underground races know how to fight in tunnels.
- Throwable explosives: black powder bombs for mining and combat —
  directional charges, larger blasts, fuse timers.
- Drow-specific weapons and ammunition: poison-tipped bolts, alchemical
  agents (sleep, paralysis), shadow-cloth gear with stealth bonuses.

**Acceptance criteria.**
- A dwarf with a hammer has a clearly different combat feel than a gnome
  with a crossbow or a drow with poisoned bolts.
- Combat in tunnels feels like the player's home turf.

---

### Economy and Trade

**Goal.** Traders should react to who you are and feel like part of the
underground world.

**Key behaviors.**
- A new dwarven trader type spawns underground, occasionally. Sells
  dwarven-themed goods: recipes, ales, rare ingots, mold blueprints.
- Translocators reflavored as "ancient dwarven gates" — visual and
  narrative reskin of the existing system.
- Trader interactions vary by player race (see Races section).
- Reputation per race: each player builds their own standing with a
  specific trader independently.

**Acceptance criteria.**
- A Drow player has a meaningfully different shopping experience than
  a Dwarf player.
- Players can build relationships with specific traders over time.
- Underground exploration occasionally rewards the player with new
  trader contacts.

---

## Multiplayer Design

Mineholme is multiplayer-first. Every system is designed for any number
of players (one to many) on a server. The state model:

**Per-player state.** Race, inebriation magnitude, event log, loot
history, masterwork attribution, individual trader reputation. Stored
on the player entity, persisted across logout, synced to client for
HUD display.

**Per-world state.** Kingdom hero loot taken, world age, gravestones,
named forgotten beasts that have lived and died. All players see the
same value; the world has one history.

**Per-region state.** Fortress wealth (tracked per chunked region, not
per-player). A shared mead hall built by five players accumulates
collective wealth and attracts collective trouble.

**Per-instance objects.** Cracked vessels, engraved blocks, gravestones,
masterwork items. Exist once in the world; all players interact with
the same instance.

**Per-player instanced loot.** Per-player loot chests in lost kingdoms
generate fresh rolls for new players, with bounded loot tables to
prevent gear flooding.

**Cooperative defaults.** Where possible, mechanics reward cooperative
play rather than competition. A late arrival to a kingdom still finds
loot. Multiple players in a fortress build wealth faster. Mixed-race
parties have role specialization.

**Server-side authority.** All trait modifiers, intoxication effects,
and reward calculations happen server-side. Clients render results.
No client-side calculation that could be cheated.

**Configurable difficulty.** Per-server config flags control the most
contentious mechanics: cave-in severity (off / mild / hardcore), Drow
trader hostility level, kingdom density, forgotten beast spawn rates.
Server admins tune the experience for their community.

---

## Roadmap

The full vision in this spec is large. It ships in milestones, each of
which is a complete, playable mod release on its own.

**v0.1 — Flavor and Food.** Surface/underground nutrition split. Drink
satiety rebalance. Dwarven cooking recipes. Underground mob spawn
expansion. Initial decorative block palette. Translocator reflavor. A
playable mod with no C# required, released to gather early feedback.

**v0.2 — Inebriation and Brewing.** The Inebriation system, specialty
named brews, brew quality tiers. Establishes the "drink is mandatory"
identity. Mushroom ales added to the catalog.

**v0.3 — Smithing and Metallurgy.** Scrap recovery, smithing with bits,
tool head repair, expanded molds, helve hammer fixes, cave-only ore
spawns. The full smithing rework.

**v0.4 — Identity and Legacy.** Masterwork crafting, engravings,
gravestones, world ages. The systems that give the world memory.

**v0.5 — Races.** The race definition framework, Dwarf and Gnome and
Drow with full traits, race-aware nutrition, race-specific recipes,
trader interactions, depth/light comfort framework. The mod's identity
locks in here.

**v0.6 — Atmosphere and Depth.** Cast iron, magma forge, deep-cave
audio, massive chambers, underground waterfalls, light shafts, crystal
formations, bioluminescent flora, forgotten beasts, visible wealth.
The mod becomes beautiful as well as deep.

**v1.0 — Worldgen and Hard Systems.** Connected cave networks,
underground biomes, lost dwarven kingdoms with per-player loot,
cave-ins, lore-driven small ruins. The headline exploration content.

**Post-v1.0.** Underground rivers (full hydrology), Dwarven king's
missive event system, vermin/menagerie ecosystems, additional races
contributed by the community.

The order is not fixed. Some milestones may swap based on what's
exciting to build. The key constraint: each milestone ships as a
complete, stable, playable mod in its own right.

---

## Open Questions

These remain unresolved and need decisions during implementation. They
are documented here so future-you doesn't lose track.

- **VS version targeting.** Is Mineholme built for 1.21 (where most
  reference mods are stable) or 1.22 (where the ecosystem is still
  catching up)? Decision deferred to implementation; design is
  version-agnostic.

- **Save migration.** How are existing worlds and existing players
  handled when the mod is added mid-campaign? Race assignment for
  pre-existing characters needs a default or one-time prompt. World-
  state systems (kingdoms, age tracker) need a graceful bootstrap.

- **Drow naming and IP.** "Drow" originates in Scottish/Orkney folklore
  predating D&D, but the visual archetype is heavily associated with
  Wizards of the Coast. Other Vintage Story mods use the term freely.
  Risk is realistically low for a free mod, but an alternative ("Nightkin",
  "Shaderim") could be substituted while keeping the design.

- **Drow social mechanics.** Lore-canonical drow are slavers and raiders.
  Translating that into mechanics is ethically fraught. Recommended
  default: skip slavery mechanics entirely, lean on stealth/raider/light-
  averse axis.

- **Anti-griefing.** Cave-ins on multiplayer servers can be weaponized.
  Ship disabled by default with config opt-in for hardcore servers.

- **Compatibility posture.** Even without dependencies, how do we behave
  when popular mods (Smithing Plus, Better Ruins) are also installed?
  Suggest config flags to disable overlapping features.

- **Kingdom density.** Too rare and most players never find one; too
  common and they lose meaning. Config-tunable; defaults need playtesting.

- **Cooperative vs. competitive defaults.** First-finder-wins on hero
  loot is the current default. Some servers will want fully cooperative
  party loot. Config flag with party/group definition required.

- **Server tick budget.** Several systems do periodic per-region or
  per-player scans (cave-in stability, fortress wealth, masterwork
  procs). On large servers these add up. Profile early and be deliberate
  about scan frequency and scope.

---

## References

### Vintage Story modding entry points

- **Official wiki**: https://wiki.vintagestory.at/Modding:Getting_Started
- **Anego Studios GitHub**: https://github.com/anegostudios — clone the
  vsapi, vsessentialsmod, vssurvivalmod, vsmodexamples, and vsmodelcreator
  repos for source reference.
- **Reddit modding starter thread**: https://www.reddit.com/r/VintageStory/comments/1hwi4nn/how_can_i_start_making_mods/
- **ModDB**: https://mods.vintagestory.at/

### Reference mods for specific systems

- **Smithing Plus** — most comprehensive smithing rework. Reference for
  Smithing and Metallurgy section.
- **Thrifty Smithing** (Foxcapades) — clean smithing scrap recovery with
  good public-API design. Open source.
- **Salvage+** — tool salvage and bit recovery patterns.
- **All Caves Are Connected** (moskva) — worldgen post-processing for
  connected cave networks.
- **Cave-Only Ore Spawns** (moskva) — ore spawn rule patches.
- **Wildcraft** — custom plant and fungus growth behaviors.
- **SlowTox** — positive-effects intoxication system. Direct reference
  for the Inebriation system.
- **Algernon's Watersheds** — terrain-aware stream pathing. Reference
  for underground rivers as a stretch goal.
- **Better Ruins** (NicIAss, open source) — multi-chunk structures with
  600+ handcrafted layouts. Technical model for Lost Dwarven Kingdoms.
- **Just More Ruins** — smaller-scale alternative reference.
- **Racial Equality (Expanded)** + **PlayerModelLib** — race attribute
  storage patterns.
- **Extra Race Traits** (Pants_Bandit) — race trait system model.

### Cross-game inspirations

- **Dwarf Fortress** (Bay 12 Games) — primary thematic inspiration.
- **LOOTR** (Minecraft mod) — conceptual reference for per-player loot
  in shared dungeons.
- **The Lord of the Rings: Mines of Moria** — narrative reference for
  Lost Dwarven Kingdoms.
