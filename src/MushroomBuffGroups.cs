using System.Collections.Generic;
using System.Linq;

namespace Mineholme;

public enum MushroomStat { Mine, Move, Stab }

public record MushroomEntry(string VanillaCode, string LoreName, MushroomStat Stat);

public static class MushroomBuffGroups
{
    public static readonly IReadOnlyList<MushroomEntry> All = new[]
    {
        // ── Mining Speed ────────────────────────────────────────────────────
        new MushroomEntry("chanterelle",        "Forgegold",      MushroomStat.Mine),
        new MushroomEntry("kingbolete",         "Vault King",     MushroomStat.Mine),
        new MushroomEntry("commonmorel",        "Riddlecap",      MushroomStat.Mine),
        new MushroomEntry("almondmushroom",     "Stonebread",     MushroomStat.Mine),
        new MushroomEntry("fieldmushroom",      "Dustcap",        MushroomStat.Mine),
        new MushroomEntry("indigomilkcap",      "Veinbleed",      MushroomStat.Mine),
        new MushroomEntry("shiitake",           "Deepbark",       MushroomStat.Mine),
        new MushroomEntry("saffronmilkcap",     "Ironmilk",       MushroomStat.Mine),
        new MushroomEntry("lobster",            "Bloodwrap",      MushroomStat.Mine),
        new MushroomEntry("earthball",          "Stonebulb",      MushroomStat.Mine),
        new MushroomEntry("golddropmilkcap",    "Gilttear",       MushroomStat.Mine),
        new MushroomEntry("beardedtooth",       "Stonefang",      MushroomStat.Mine),
        new MushroomEntry("chickenofthewoods",  "Rockbracket",    MushroomStat.Mine),
        new MushroomEntry("dryadsaddle",        "Gargoylemantle", MushroomStat.Mine),
        new MushroomEntry("tinderhoof",         "Ashshelf",       MushroomStat.Mine),

        // ── Movement Speed ──────────────────────────────────────────────────
        new MushroomEntry("bitterbolete",       "Ashcurd",        MushroomStat.Move),
        new MushroomEntry("blacktrumpet",       "Hollowhorn",     MushroomStat.Move),
        new MushroomEntry("elfinsaddle",        "Boneback",       MushroomStat.Move),
        new MushroomEntry("greencrackedrussula","Moldvein",        MushroomStat.Move),
        new MushroomEntry("orangeoakbolete",    "Rustking",       MushroomStat.Move),
        new MushroomEntry("puffball",           "Dustsphere",     MushroomStat.Move),
        new MushroomEntry("redwinecap",         "Bloodtop",       MushroomStat.Move),
        new MushroomEntry("goldcap",            "Gleamcap",       MushroomStat.Move),
        new MushroomEntry("honeymushroom",      "Dripgold",       MushroomStat.Move),
        new MushroomEntry("wavycap",            "Mindrift",       MushroomStat.Move),
        new MushroomEntry("pinkoyster",         "Blushear",       MushroomStat.Move),
        new MushroomEntry("whiteoyster",        "Ghostshelf",     MushroomStat.Move),
        new MushroomEntry("paddystraw",         "Dunestalk",      MushroomStat.Move),
        new MushroomEntry("sickener",           "Gutcurl",        MushroomStat.Move),
        new MushroomEntry("deerear",            "Batear",         MushroomStat.Move),

        // ── Temporal Stability ──────────────────────────────────────────────
        new MushroomEntry("flyagaric",          "Veilspot",       MushroomStat.Stab),
        new MushroomEntry("deathcap",           "Gravecap",       MushroomStat.Stab),
        new MushroomEntry("devilstooth",        "Jawrock",        MushroomStat.Stab),
        new MushroomEntry("devilbolete",        "Hellking",       MushroomStat.Stab),
        new MushroomEntry("jackolantern",       "Embercrown",     MushroomStat.Stab),
        new MushroomEntry("violetwebcap",       "Duskweave",      MushroomStat.Stab),
        new MushroomEntry("witchhat",           "Cronecap",       MushroomStat.Stab),
        new MushroomEntry("bluemeanie",         "Duskshade",      MushroomStat.Stab),
        new MushroomEntry("foolsconecap",       "Doombonnet",     MushroomStat.Stab),
        new MushroomEntry("laughingjim",        "Madcap",         MushroomStat.Stab),
        new MushroomEntry("libertycap",         "Shacklebell",    MushroomStat.Stab),
        new MushroomEntry("reishi",             "Lacquercap",     MushroomStat.Stab),
        new MushroomEntry("funeralbell",        "Deathknell",     MushroomStat.Stab),
        new MushroomEntry("livermushroom",      "Marrowcap",      MushroomStat.Stab),
        new MushroomEntry("pinkbonnet",         "Blushveil",      MushroomStat.Stab),
    };

    // Indexed lookups built once at startup
    static readonly Dictionary<string, MushroomEntry> ByVanillaCode =
        All.ToDictionary(e => e.VanillaCode);

    static readonly Dictionary<string, MushroomEntry> ByLoreName =
        All.ToDictionary(e => e.LoreName);

    public static MushroomEntry? GetByVanillaCode(string code) =>
        ByVanillaCode.TryGetValue(code, out var e) ? e : null;

    public static MushroomEntry? GetByLoreName(string name) =>
        ByLoreName.TryGetValue(name, out var e) ? e : null;

    // Returns "mine", "move", "stab", or null — used by DrinkConsumptionPatch
    public static string? GetStat(string vanillaCode) =>
        ByVanillaCode.TryGetValue(vanillaCode, out var e) ? StatKey(e.Stat) : null;

    public static string StatKey(MushroomStat stat) => stat switch
    {
        MushroomStat.Mine => "mine",
        MushroomStat.Move => "move",
        MushroomStat.Stab => "stab",
        _                 => "mine"
    };
}
