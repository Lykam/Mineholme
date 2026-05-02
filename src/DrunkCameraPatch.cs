using Vintagestory.API.Client;

namespace Mineholme;

// Applied manually in MinehomeMod.StartClientSide — no [HarmonyPatch] attribute
// so PatchAll() in Start() does not attempt to resolve this on a dedicated server.
public static class DrunkCameraPatch
{
    public static bool Prefix(DrunkPerceptionEffect __instance)
    {
        __instance.Intensity = 0f;
        MinehomeMod.ClientApi?.Render.ShaderUniforms
            .AmbientBloomLevelAdd[DefaultShaderUniforms.BloomAddDrunkIndex] = 0f;
        return false;
    }
}
