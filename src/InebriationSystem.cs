using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Mineholme;

public class InebriationSystem : ModSystem
{
    ICoreServerAPI sapi = null!;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;
        api.Event.RegisterGameTickListener(OnTick, 1000);
    }

    private void OnTick(float dt)
    {
        foreach (IPlayer player in sapi.World.AllOnlinePlayers)
        {
            var entity = player.Entity;
            if (entity == null || !entity.Alive) continue;
            UpdateEffects(entity, dt);
        }
    }

    private void UpdateEffects(Entity entity, float dt)
    {
        // Decay rate matches vanilla: BehaviorHunger decays 0.005 * gameSpeedMul per second
        float gameSpeed = (float)(sapi.World.Calendar.SpeedOfTime * sapi.World.Calendar.CalendarSpeedMul / 30.0);
        if (gameSpeed == 0f) gameSpeed = 1f;
        float decay = 0.005f * gameSpeed * dt;

        // Generic (vanilla + mushroomaleportion) — sober penalty removal only
        float gf = DecayAttr(entity, "mh_intox_f", decay);
        float gd = DecayAttr(entity, "mh_intox_d", decay);
        float ga = DecayAttr(entity, "mh_intox_a", decay);

        // Mining mushrooms
        float mf = DecayAttr(entity, "mh_mine_f", decay);
        float md = DecayAttr(entity, "mh_mine_d", decay);
        float ma = DecayAttr(entity, "mh_mine_a", decay);

        // Movement mushrooms
        float vf = DecayAttr(entity, "mh_move_f", decay);
        float vd = DecayAttr(entity, "mh_move_d", decay);
        float va = DecayAttr(entity, "mh_move_a", decay);

        // Stability mushrooms
        float sf = DecayAttr(entity, "mh_stab_f", decay);
        float sd = DecayAttr(entity, "mh_stab_d", decay);
        float sa = DecayAttr(entity, "mh_stab_a", decay);

        // Sober penalty fires when ALL attributes are below 0.4.
        // Any drink — even generic vanilla ale — is enough to remove it.
        float soberIntox = MathF.Max(gf, MathF.Max(gd, MathF.Max(ga,
                           MathF.Max(mf, MathF.Max(md, MathF.Max(ma,
                           MathF.Max(vf, MathF.Max(vd, MathF.Max(va,
                           MathF.Max(sf, MathF.Max(sd, sa)))))))))));

        float depth = Math.Max(0f, sapi.World.SeaLevel - (float)entity.Pos.Y);
        float depthScale = 1f + Math.Min(1f, depth / 150f); // 1.0 surface → 2.0 at depth 150

        entity.Stats.Set("miningSpeedMul", "mineholme_inebriation", MiningDelta(mf, md, ma, soberIntox, depthScale));
        entity.Stats.Set("walkspeed",      "mineholme_inebriation", MoveDelta(vf, vd, va, soberIntox));

        UpdateTemporalStability(entity, sf, sd, sa, soberIntox, depthScale, dt);
    }

    // -----------------------------------------------------------------------
    // Stat formulas
    //
    // Sober penalty: based on soberIntox (max across all 12 attributes).
    //   Any drink removes it. The penalty is the same regardless of which
    //   mushroom type you drink — only the BUFF is stat-specific.
    //
    // Mining:  sober -30% (depth-amplified) → peak +10/20/30% for f/d/a mining mushrooms
    // Move:    sober -15%                   → peak +5/10/15% for f/d/a movement mushrooms
    // Stability: see UpdateTemporalStability
    // -----------------------------------------------------------------------

    private static float MiningDelta(float f, float d, float a, float soberIntox, float depthScale)
    {
        return SoberPenalty(soberIntox, -0.30f, depthScale)
             + TierBuff(f, 0.10f)
             + TierBuff(d, 0.20f)
             + TierBuff(a, 0.30f);
    }

    private static float MoveDelta(float f, float d, float a, float soberIntox)
    {
        return SoberPenalty(soberIntox, -0.15f, 1f)
             + TierBuff(f, 0.05f)
             + TierBuff(d, 0.10f)
             + TierBuff(a, 0.15f);
    }

    private static void UpdateTemporalStability(Entity entity,
        float sf, float sd, float sa, float soberIntox, float depthScale, float dt)
    {
        double stability = entity.WatchedAttributes.GetDouble("temporalStability", 1.0);

        if (soberIntox < 0.4f)
        {
            // Sober: extra stability drain scales with depth and how sober you are
            float soberFraction = 1f - soberIntox / 0.4f;
            double drain = 0.002 * soberFraction * depthScale * dt;
            entity.WatchedAttributes.SetDouble("temporalStability", Math.Max(0.0, stability - drain));
        }
        else if (stability < 1.0)
        {
            // Inebriated: stability regen from stability mushrooms only
            double regen = (TierRegenRate(sf, 0.001) + TierRegenRate(sd, 0.002) + TierRegenRate(sa, 0.003)) * dt;
            if (regen > 0)
                entity.WatchedAttributes.SetDouble("temporalStability", Math.Min(1.0, stability + regen));
        }
    }

    // -----------------------------------------------------------------------
    // Shared helpers
    // -----------------------------------------------------------------------

    // Negative delta from floor to 0 as intox rises 0→0.4. Amplified by depthScale.
    private static float SoberPenalty(float intox, float floor, float depthScale)
    {
        if (intox >= 0.4f) return 0f;
        return GameMath.Lerp(floor, 0f, intox / 0.4f) * depthScale;
    }

    // Sweet-spot buff: 0 at intox 0.4, ramps to peak at 0.75, back to 0 at 1.1.
    private static float TierBuff(float intox, float peak)
    {
        if (intox <= 0.4f) return 0f;
        if (intox <= 0.75f) return GameMath.Lerp(0f, peak, (intox - 0.4f) / 0.35f);
        return GameMath.Lerp(peak, 0f, (intox - 0.75f) / 0.35f);
    }

    // Stability regen rate for one tier, ramping in the sweet spot.
    private static double TierRegenRate(float intox, double maxRate)
    {
        if (intox <= 0.4f) return 0;
        if (intox <= 0.75f) return maxRate * GameMath.Lerp(0f, 1f, (intox - 0.4f) / 0.35f);
        return maxRate * GameMath.Lerp(1f, 0f, (intox - 0.75f) / 0.35f);
    }

    private static float DecayAttr(Entity entity, string key, float amount)
    {
        float current = entity.WatchedAttributes.GetFloat(key);
        if (current <= 0f) return 0f;
        float next = Math.Max(0f, current - amount);
        entity.WatchedAttributes.SetFloat(key, next);
        return next;
    }
}
