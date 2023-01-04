using System.Collections.Generic;
using HarmonyLib;
using OpenTK.Graphics.ES30;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace ReRender;

[HarmonyPatch(typeof(ClientMain))]
public static class ClientMainPatches
{
    [HarmonyPatch("MainRenderLoop")]
    [HarmonyPrefix]
    private static bool MainRenderLoopPrefix(float dt, ClientMain __instance)
    {
        var engine = ReRenderMod.Instance?.RenderEngine;
        if (engine == null) return true;
        engine.RunMainRenderCycle(dt, __instance);
        return false;
    }
}

[HarmonyPatch(typeof(ClientPlatformWindows))]
public static class ClientPlatformWindowsPatches
{
    [HarmonyPatch("ClearFrameBuffer", typeof(EnumFrameBuffer))]
    [HarmonyPrefix]
    private static bool ClearFrameBufferPrefix(EnumFrameBuffer framebuffer)
    {
        // block primary and transparent clears
        return framebuffer != EnumFrameBuffer.Primary && framebuffer != EnumFrameBuffer.Transparent;
    }

    [HarmonyPatch("RenderPostprocessingEffects")]
    [HarmonyPrefix]
    private static bool RenderPostprocessingEffectsPrefix(ClientPlatformWindows __instance, MeshRef ___screenQuad)
    {
        GL.Enable(EnableCap.Blend);
        return false;
    }

    [HarmonyPatch("RenderFinalComposition")]
    [HarmonyPrefix]
    private static bool RenderFinalCompositionPrefix(ClientPlatformWindows __instance, MeshRef ___screenQuad)
    {
        return false;
    }

    [HarmonyPatch("SetupDefaultFrameBuffers")]
    [HarmonyPostfix]
    private static void SetupDefaultFrameBuffersPostfix(List<FrameBufferRef> __result, MeshRef ___screenQuad)
    {
        ReRenderMod.Instance?.RenderEngine?.AfterFramebufferInit(__result, ___screenQuad);
    }
}