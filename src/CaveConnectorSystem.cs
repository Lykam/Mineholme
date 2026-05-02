using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Mineholme;

// Connects all cave systems in a chunk column via a two-phase approach:
//   Phase 1 — Backbone: carves organic tunnels from per-band "nodes" inside the chunk
//             to deterministic boundary points on chunk faces, so adjacent chunks always meet.
//   Phase 2 — Components: flood-fills cave air, then connects each isolated pocket to the
//             nearest backbone node via an organic tunnel.
// Runs at ExecuteOrder 0.45 inside the TerrainFeatures pass, after vanilla cave carving.
// Algorithm adapted from AllCavesAreConnected by Vinter_Nacht (mod 48178).
public class CaveConnectorSystem : ModSystem
{
    // ── nested types ──────────────────────────────────────────────
    readonly struct TP(double x, double y, double z)
    {
        public readonly double X = x, Y = y, Z = z;
    }
    readonly struct TV(double x, double y, double z)
    {
        public readonly double X = x, Y = y, Z = z;
    }
    readonly struct VP(int x, int y, int z)
    {
        public readonly int X = x, Y = y, Z = z;
    }
    sealed class AirComp
    {
        public int Id, Count;
        public long SumX, SumY, SumZ;
        public readonly List<VP> Samples = new();
        public readonly List<VP> WallSamples = new();
        public VP Anchor;
    }

    // ── constants ─────────────────────────────────────────────────
    const int TunnelWidth    = 3;
    const int MaxConnectDist = 64;
    const int MaxSamples     = 24;
    const int SurfaceBuf     = 6;
    const int PocketRoofScan = 6;
    const int NSPeriod       = 4;  // N/S spines every N chunk columns
    const int ShaftPeriod    = 3;  // vertical shafts every S×S chunks

    // ── fields ────────────────────────────────────────────────────
    ICoreServerAPI sapi = null!;
    int seed, worldH, cs;
    readonly List<int> bands = new();

    // ── ModSystem ─────────────────────────────────────────────────
    public override double ExecuteOrder() => 0.45;
    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi   = api;
        seed   = api.WorldManager.Seed;
        worldH = api.WorldManager.MapSizeY;
        cs     = api.WorldManager.ChunkSize;
        BuildBands();
        api.Event.ChunkColumnGeneration(OnGen, EnumWorldGenPass.TerrainFeatures, "standard");
    }

    void BuildBands()
    {
        int step = Math.Clamp(MaxConnectDist - 16, 24, 48);
        for (int y = Math.Min(worldH - 8, 12); y < worldH - 12; y += step)
            bands.Add(y);
        int top = Math.Max(12, worldH - 20);
        if (bands.Count == 0 || bands[^1] < top - 8)
            bands.Add(top);
    }

    // ── main entry ────────────────────────────────────────────────
    void OnGen(IChunkColumnGenerateRequest req)
    {
        if (req.Chunks == null || req.Chunks.Length == 0) return;
        IServerChunk? first = null;
        foreach (var c in req.Chunks) if (c != null) { first = c; break; }
        IMapChunk? mapChunk = first != null ? ((IWorldChunk)first).MapChunk : null;
        if (mapChunk == null) return;

        int cx = req.ChunkX, cz = req.ChunkZ;
        var chunks = req.Chunks;
        bool[] dirty = new bool[chunks.Length];

        var surf    = BuildSurf(mapChunk.WorldGenTerrainHeightMap);
        var airMask = BuildAirMask(chunks, surf);
        var comps   = FindComponents(chunks, airMask);
        var nodes   = MakeNodes(cx, cz);

        Backbone(chunks, surf, cx, cz, nodes, dirty);
        Connect(chunks, surf, cx, cz, comps, nodes, dirty);

        for (int i = 0; i < chunks.Length; i++)
            if (dirty[i] && chunks[i] != null)
                ((IWorldChunk)chunks[i]).MarkModified();
    }

    // ── surface limits ────────────────────────────────────────────
    ushort[] BuildSurf(ushort[]? hmap)
    {
        var s = new ushort[cs * cs];
        for (int z = 0; z < cs; z++)
        for (int x = 0; x < cs; x++)
        {
            int i = z * cs + x;
            int h = (hmap != null && hmap.Length > i) ? hmap[i] : worldH - 2;
            if (h <= 0) h = worldH - 2;
            s[i] = (ushort)Math.Clamp(h - SurfaceBuf, 2, worldH - 2);
        }
        return s;
    }

    // ── cave air classification ───────────────────────────────────
    byte[] BuildAirMask(IServerChunk[] chunks, ushort[] surf)
    {
        var m = new byte[cs * cs * worldH];
        for (int y = 2; y < worldH - 1; y++)
        for (int z = 0; z < cs; z++)
        for (int x = 0; x < cs; x++)
            if (IsDryAir(chunks, x, y, z) && IsUnderground(chunks, surf, x, y, z))
                m[Flat(x, y, z)] = 1;
        return m;
    }

    bool IsDryAir(IServerChunk[] chunks, int x, int y, int z)
    {
        if (!InCol(x, y, z)) return false;
        int ci = y / cs;
        if (chunks[ci] == null) return false;
        int idx = Idx(x, y % cs, z);
        var d = ((IWorldChunk)chunks[ci]).Data;
        return d[idx] == 0 && d.GetFluid(idx) == 0;
    }

    bool IsUnderground(IServerChunk[] chunks, ushort[] surf, int x, int y, int z)
    {
        if (y <= surf[z * cs + x]) return true;
        return HasRoof(chunks, x, y, z, PocketRoofScan) && SolidNeighbors(chunks, x, y, z) >= 3;
    }

    bool CanCarve(IServerChunk[] chunks, ushort[] surf, int x, int y, int z)
    {
        if (!InBounds(x, y, z)) return false;
        return y <= surf[z * cs + x] || HasRoof(chunks, x, y, z, PocketRoofScan);
    }

    bool HasRoof(IServerChunk[] chunks, int x, int y, int z, int dist)
    {
        for (int i = 1; i <= dist; i++) if (BlockAt(chunks, x, y + i, z) != 0) return true;
        return false;
    }

    int SolidNeighbors(IServerChunk[] chunks, int x, int y, int z)
    {
        int n = 0;
        if (BlockAt(chunks, x - 1, y, z) != 0) n++;
        if (BlockAt(chunks, x + 1, y, z) != 0) n++;
        if (BlockAt(chunks, x, y - 1, z) != 0) n++;
        if (BlockAt(chunks, x, y + 1, z) != 0) n++;
        if (BlockAt(chunks, x, y, z - 1) != 0) n++;
        if (BlockAt(chunks, x, y, z + 1) != 0) n++;
        return n;
    }

    // ── flood-fill component detection ────────────────────────────
    List<AirComp> FindComponents(IServerChunk[] chunks, byte[] airMask)
    {
        var seen = new byte[airMask.Length];
        var q    = new Queue<int>();
        var list = new List<AirComp>();

        for (int y = 2; y < worldH - 1; y++)
        for (int z = 0; z < cs; z++)
        for (int x = 0; x < cs; x++)
        {
            int fi = Flat(x, y, z);
            if (airMask[fi] == 0 || seen[fi] != 0) continue;

            var comp = new AirComp { Id = list.Count };
            seen[fi] = 1;
            q.Enqueue(fi);

            while (q.Count > 0)
            {
                var p = Unflat(q.Dequeue());
                comp.Count++;
                comp.SumX += p.X; comp.SumY += p.Y; comp.SumZ += p.Z;
                Sample(comp, p);
                if (IsWallAdjacent(chunks, p.X, p.Y, p.Z)) WallSample(comp, p);
                Flood(airMask, seen, q, p.X - 1, p.Y, p.Z);
                Flood(airMask, seen, q, p.X + 1, p.Y, p.Z);
                Flood(airMask, seen, q, p.X, p.Y - 1, p.Z);
                Flood(airMask, seen, q, p.X, p.Y + 1, p.Z);
                Flood(airMask, seen, q, p.X, p.Y, p.Z - 1);
                Flood(airMask, seen, q, p.X, p.Y, p.Z + 1);
            }
            comp.Anchor = Centroid(comp);
            list.Add(comp);
        }
        return list;
    }

    void Flood(byte[] mask, byte[] seen, Queue<int> q, int x, int y, int z)
    {
        if (x < 0 || x >= cs || z < 0 || z >= cs || y < 2 || y >= worldH - 1) return;
        int fi = Flat(x, y, z);
        if (mask[fi] != 0 && seen[fi] == 0) { seen[fi] = 1; q.Enqueue(fi); }
    }

    void Sample(AirComp c, VP p)
    {
        if (c.Samples.Count < MaxSamples) { c.Samples.Add(p); return; }
        int s = (int)(H01(p.X, p.Y, p.Z, c.Id, c.Count) * c.Count);
        if (s < MaxSamples) c.Samples[s] = p;
    }

    void WallSample(AirComp c, VP p)
    {
        if (c.WallSamples.Count < MaxSamples) { c.WallSamples.Add(p); return; }
        int s = (int)(H01(p.X, p.Y, p.Z, c.Id, c.Count, 811) * c.Count);
        if (s < MaxSamples) c.WallSamples[s] = p;
    }

    bool IsWallAdjacent(IServerChunk[] chunks, int x, int y, int z)
        => BlockAt(chunks, x - 1, y, z) != 0 || BlockAt(chunks, x + 1, y, z) != 0
        || BlockAt(chunks, x, y, z - 1) != 0  || BlockAt(chunks, x, y, z + 1) != 0;

    VP Centroid(AirComp c)
    {
        if (c.Samples.Count == 0) return default;
        double mcx = (double)c.SumX / c.Count, mcy = (double)c.SumY / c.Count, mcz = (double)c.SumZ / c.Count;
        VP best = c.Samples[0];
        double bd = double.MaxValue;
        foreach (var p in c.Samples)
        {
            double d = (p.X - mcx) * (p.X - mcx) + (p.Y - mcy) * (p.Y - mcy) + (p.Z - mcz) * (p.Z - mcz);
            if (d < bd) { bd = d; best = p; }
        }
        return best;
    }

    // ── band nodes ────────────────────────────────────────────────
    (int bi, TP pt)[] MakeNodes(int cx, int cz)
    {
        var nodes = new (int, TP)[bands.Count];
        for (int i = 0; i < bands.Count; i++) nodes[i] = (i, NodePt(cx, cz, i));
        return nodes;
    }

    TP NodePt(int cx, int cz, int bi)
    {
        double m = cs * 0.25, inner = cs - m * 2;
        double x = m + H01(cx, cz, bi, 11) * inner;
        double z = m + H01(cx, cz, bi, 23) * inner;
        double y = bands[bi] + HS(cx, cz, bi, 37) * 6.0;
        return new TP(x, Math.Clamp(y, 4, worldH - 4), z);
    }

    // dir: 0=West(−X), 1=East(+X), 2=South(−Z), 3=North(+Z)
    // Boundary point is agreed upon by both adjacent chunks via canonical min(coord) key.
    TP BoundPt(int cx, int cz, int bi, int dir)
    {
        int nx = cx + (dir == 1 ? 1 : dir == 0 ? -1 : 0);
        int nz = cz + (dir == 3 ? 1 : dir == 2 ? -1 : 0);
        int canX = Math.Min(cx, nx), canZ = Math.Min(cz, nz);
        int ch = dir <= 1 ? 0 : 1;

        double m = Math.Max(2.0, cs * 0.25 - 2.0), inner = cs - m * 2;
        double off = m + H01(canX, canZ, bi, ch, 101) * inner;
        double y = (NodePt(cx, cz, bi).Y + NodePt(nx, nz, bi).Y) * 0.5
                   + HS(canX, canZ, bi, ch, 131) * 3.0;
        y = Math.Clamp(y, 4, worldH - 4);
        const double e = 0.65;
        return dir switch
        {
            0 => new TP(e,      y, off),
            1 => new TP(cs - e, y, off),
            2 => new TP(off,    y, e),
            _ => new TP(off,    y, cs - e),
        };
    }

    // ── backbone ─────────────────────────────────────────────────
    void Backbone(IServerChunk[] chunks, ushort[] surf, int cx, int cz, (int bi, TP pt)[] nodes, bool[] dirty)
    {
        int w = Math.Max(2, Math.Min(TunnelWidth, 3));
        for (int i = 0; i < nodes.Length; i++)
        {
            var (bi, node) = nodes[i];
            // East and West: always connected at every chunk
            Organic(chunks, surf, node, BoundPt(cx, cz, bi, 0), w, dirty, cx, cz, 2000 + i * 17 + 1);
            Organic(chunks, surf, node, BoundPt(cx, cz, bi, 1), w, dirty, cx, cz, 2000 + i * 17 + 2);
            // North and South: every NSPeriod chunk columns, staggered by band index
            if (PosMod(cx + bi, NSPeriod) == 0)
            {
                Organic(chunks, surf, node, BoundPt(cx, cz, bi, 2), w, dirty, cx, cz, 2000 + i * 17 + 3);
                Organic(chunks, surf, node, BoundPt(cx, cz, bi, 3), w, dirty, cx, cz, 2000 + i * 17 + 4);
            }
        }
        // Vertical shafts: connect band levels every ShaftPeriod×ShaftPeriod chunk grid
        if (PosMod(cx, ShaftPeriod) == 0 && PosMod(cz, ShaftPeriod) == 0)
            for (int j = 0; j < nodes.Length - 1; j++)
                Organic(chunks, surf, nodes[j].pt, nodes[j + 1].pt, w, dirty, cx, cz, 7000 + j * 29);
    }

    // ── component connection ──────────────────────────────────────
    void Connect(IServerChunk[] chunks, ushort[] surf, int cx, int cz, List<AirComp> comps, (int bi, TP pt)[] nodes, bool[] dirty)
    {
        int w = Math.Max(2, TunnelWidth);
        foreach (var comp in comps)
        {
            var nearest = NearestNode(ToTP(comp.Anchor), nodes);
            var portal  = ToTP(Portal(comp, nearest.pt));
            if (DSq(portal, nearest.pt) < 1.0) continue;
            var bDir = Norm(Sub(nearest.pt, portal));
            // Short straight breach to punch cleanly through the cave wall
            Straight(chunks, surf, Add(portal, Mul(bDir, -0.45)), Add(portal, Mul(bDir, 1.15)), w, dirty, cx, cz, 90000 + comp.Id * 17);
            Organic(chunks, surf, portal, nearest.pt, w, dirty, cx, cz, 10000 + comp.Id * 131);
        }
    }

    VP Portal(AirComp comp, TP target)
    {
        var list = comp.WallSamples.Count > 0 ? comp.WallSamples : comp.Samples;
        if (list.Count == 0) return comp.Anchor;
        VP best = list[0];
        double bd = double.MaxValue;
        foreach (var p in list) { double d = DSq(ToTP(p), target); if (d < bd) { bd = d; best = p; } }
        return best;
    }

    (int bi, TP pt) NearestNode(TP anchor, (int bi, TP pt)[] nodes)
    {
        var best = nodes[0]; double bd = double.MaxValue;
        foreach (var n in nodes) { double d = DSq(anchor, n.pt); if (d < bd) { bd = d; best = n; } }
        return best;
    }

    // ── organic tunnel ────────────────────────────────────────────
    // Straight-line walk with sinusoidal perpendicular displacement driven by
    // smooth curve noise; endpoints are pinned (sin(0) = sin(π) = 0) so tunnels
    // always meet their start/end positions exactly.
    void Organic(IServerChunk[] chunks, ushort[] surf, TP start, TP end, int w, bool[] dirty, int cx, int cz, int salt)
    {
        w = Math.Clamp(w, 1, 5);
        var dir = Sub(end, start);
        double len = Len(dir);
        if (len < 0.25) return;

        var fwd   = Norm(dir);
        // Fallback up-vector for near-vertical tunnels
        var up    = Math.Abs(fwd.Y) > 0.85 ? new TV(1, 0, 0) : new TV(0, 1, 0);
        var right = Norm(Cross(fwd, up));
        var rup   = Norm(Cross(fwd, right));

        int steps = Math.Max(10, (int)Math.Ceiling(len * 2.35));
        for (int i = 0; i <= steps; i++)
        {
            double t  = Smooth((double)i / steps);
            var    pt = Lerp(start, end, t);
            double sv = Math.Sin(t * Math.PI);
            pt = Add(pt, Mul(right, sv * (0.9  + w * 0.25) * Noise(cx, cz, salt, t, 1)));
            pt = Add(pt, Mul(rup,   sv * (0.45 + w * 0.12) * Noise(cx, cz, salt, t, 2)));
            double r = Math.Clamp((0.68 + (w - 1) * 0.48) * (0.88 + 0.18 * Noise(cx, cz, salt, t, 3)), 0.65, w + 0.55);
            Brush(chunks, surf, pt, r, w, dirty, salt + i * 13);
        }
    }

    void Straight(IServerChunk[] chunks, ushort[] surf, TP start, TP end, int w, bool[] dirty, int cx, int cz, int salt)
    {
        double len = Len(Sub(end, start));
        if (len < 0.05) return;
        w = Math.Clamp(w, 1, 5);
        int steps = Math.Max(3, (int)Math.Ceiling(len * 4.0));
        double r = Math.Clamp(0.72 + (w - 1) * 0.48, 0.8, w + 0.55);
        for (int i = 0; i <= steps; i++)
            Brush(chunks, surf, Lerp(start, end, (double)i / steps), r, w, dirty, salt + i * 7);
    }

    // ── ellipsoidal brush ─────────────────────────────────────────
    // Asymmetric: taller ceiling than floor, fuzzy edge via per-voxel hash.
    // Skips fluid blocks to avoid draining underground water.
    void Brush(IServerChunk[] chunks, ushort[] surf, TP c, double r, int w, bool[] dirty, int salt)
    {
        double yr   = Math.Max(1.22, r * 0.96 + 0.42);
        double r2   = r * r, yr2 = yr * yr;
        double iXZ  = Math.Max(0.55, Math.Min(r * 0.88, r + 0.1));
        const double floor = -0.82;
        double ceil = 1.38 + 0.1 * Math.Max(0, w - 2);

        int x0 = Math.Max(0,          (int)Math.Floor(c.X - r - 1));
        int x1 = Math.Min(cs - 1,     (int)Math.Ceiling(c.X + r + 1));
        int y0 = Math.Max(2,           (int)Math.Floor(c.Y + Math.Min(floor, -yr) - 1));
        int y1 = Math.Min(worldH - 2,  (int)Math.Ceiling(c.Y + Math.Max(ceil,  yr) + 1));
        int z0 = Math.Max(0,           (int)Math.Floor(c.Z - r - 1));
        int z1 = Math.Min(cs - 1,      (int)Math.Ceiling(c.Z + r + 1));

        for (int y = y0; y <= y1; y++)
        for (int z = z0; z <= z1; z++)
        for (int x = x0; x <= x1; x++)
        {
            if (!CanCarve(chunks, surf, x, y, z)) continue;
            double dx = x + 0.5 - c.X, dy = y + 0.5 - c.Y, dz = z + 0.5 - c.Z;
            double el  = (dx * dx + dz * dz) / r2 + dy * dy / yr2;
            bool   box = Math.Abs(dx) <= iXZ && Math.Abs(dz) <= iXZ && dy >= floor && dy <= ceil;
            if (!box && el > 1.0 + 0.16 * HS(x, y, z, salt)) continue;
            int ci = y / cs;
            if (chunks[ci] == null) continue;
            int idx = Idx(x, y % cs, z);
            var d = ((IWorldChunk)chunks[ci]).Data;
            if (d.GetFluid(idx) == 0 && d[idx] != 0)
            {
                d.SetBlockAir(idx);
                dirty[ci] = true;
            }
        }
    }

    // ── block access ─────────────────────────────────────────────
    int BlockAt(IServerChunk[] chunks, int x, int y, int z)
    {
        if (x < 0 || x >= cs || z < 0 || z >= cs || y < 0 || y >= worldH) return 0;
        int ci = y / cs;
        if (chunks[ci] == null) return 0;
        return ((IWorldChunk)chunks[ci]).Data[Idx(x, y % cs, z)];
    }

    bool InCol(int x, int y, int z)    => x >= 0 && x < cs && z >= 0 && z < cs && y >= 2 && y < worldH - 1;
    bool InBounds(int x, int y, int z) => x >= 0 && x < cs && z >= 0 && z < cs && y >= 2 && y < worldH - 1;

    // ── index helpers ─────────────────────────────────────────────
    int  Flat(int x, int y, int z) => (y * cs + z) * cs + x;
    VP Unflat(int fi)              => new(fi % cs, fi / cs / cs, fi / cs % cs);
    int  Idx(int x, int ly, int z) => (ly * cs + z) * cs + x;

    // ── vector math ───────────────────────────────────────────────
    static TP   Lerp(TP a, TP b, double t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
    static TP   Add(TP p, TV v)            => new(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
    static TV   Sub(TP to, TP from)        => new(to.X - from.X, to.Y - from.Y, to.Z - from.Z);
    static TV   Mul(TV v, double f)        => new(v.X * f, v.Y * f, v.Z * f);
    static double Len(TV v)               => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    static TV   Norm(TV v)                { double l = Len(v); return l < 1e-6 ? new(0, 0, 0) : Mul(v, 1 / l); }
    static TV   Cross(TV a, TV b)         => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    static double Smooth(double t)        => t * t * (3 - 2 * t);
    static double DSq(TP a, TP b)         { double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z; return dx*dx + dy*dy + dz*dz; }
    static TP   ToTP(VP p)                => new(p.X + 0.5, p.Y + 0.5, p.Z + 0.5);
    static int  PosMod(int v, int d)      { int r = v % d; return r >= 0 ? r : r + d; }

    // ── curve noise ───────────────────────────────────────────────
    // Piecewise smooth interpolation between 4 signed hash values along t ∈ [0,1].
    double Noise(int cx, int cz, int salt, double t, int ch)
    {
        double tc = t * 4.0;
        int    seg = (int)Math.Floor(tc);
        double f   = Smooth(tc - seg);
        return HS(cx, cz, salt, ch, seg) * (1 - f) + HS(cx, cz, salt, ch, seg + 1) * f;
    }

    // ── hash (FNV-inspired + SplitMix64, seeded by world seed) ───
    double H01(params int[] v)
    {
        ulong h = 1469598103934665603UL ^ (uint)seed;
        h *= 1099511628211UL;
        for (int i = 0; i < v.Length; i++)
        {
            unchecked { h ^= (ulong)((long)(uint)v[i] + (long)(i + 1) * -7046029288634856825L); }
            h *= 1099511628211UL;
            h  = SMix(h);
        }
        return (double)(h >> 11) * (1.0 / 9007199254740992.0);
    }

    double HS(params int[] v) => H01(v) * 2.0 - 1.0;

    static ulong SMix(ulong x)
    {
        x += 11400714819323198485UL;
        x  = (x ^ (x >> 30)) * 13787848793156543929UL;
        x  = (x ^ (x >> 27)) * 10723151780598845931UL;
        return x ^ (x >> 31);
    }
}
