using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Mineholme;

public class MinehomeMod : ModSystem
{
    private Harmony? harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.RegisterBlockClass("BlockCavePlant", typeof(BlockCavePlant));
        api.RegisterBlockClass("BlockCaveWallMushroom", typeof(BlockCaveWallMushroom));

        harmony = new Harmony("mineholme");
        harmony.PatchAll();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
    }

    public override void Dispose()
    {
        harmony?.UnpatchAll("mineholme");
        base.Dispose();
    }
}
