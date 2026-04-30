using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Mineholme;

public class MinehomeMod : ModSystem
{
    public static ICoreClientAPI? ClientApi;

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
        ClientApi = api;

        // Applied here rather than via PatchAll so the DrunkPerceptionEffect type
        // is never resolved on a dedicated server.
        harmony?.Patch(
            AccessTools.Method(typeof(DrunkPerceptionEffect), nameof(DrunkPerceptionEffect.OnBeforeGameRender)),
            prefix: new HarmonyMethod(typeof(DrunkCameraPatch), nameof(DrunkCameraPatch.Prefix))
        );
    }

    public override void Dispose()
    {
        harmony?.UnpatchAll("mineholme");
        ClientApi = null;
        base.Dispose();
    }
}
