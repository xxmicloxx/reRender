using HarmonyLib;
using ReRender.Engine;
using ReRender.Graph;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client;

namespace ReRender;

public class ReRenderMod : ModSystem
{
    private Harmony? _harmony;
    public static ReRenderMod? Instance { get; private set; }
    public ICoreClientAPI? Api { get; private set; }
    public RenderGraph? RenderGraph { get; private set; }
    public ReRenderEngine? RenderEngine { get; private set; }
    public VanillaRenderGraph? VanillaRenderGraph { get; private set; }

    public override void StartClientSide(ICoreClientAPI api)
    {
        Api = api;
        Instance = this;

        RenderGraph = new RenderGraph();
        RenderEngine = new ReRenderEngine(this, RenderGraph);
        VanillaRenderGraph = new VanillaRenderGraph(this, RenderGraph);

        PatchGame();

        // recreate all buffers
        ScreenManager.Platform.RebuildFrameBuffers();
    }

    private void PatchGame()
    {
        Mod.Logger.Event("Loading harmony for patching...");
        Harmony.DEBUG = true;
        _harmony = new Harmony("com.xxmicloxx.rerender");
        _harmony.PatchAll();

        var myOriginalMethods = _harmony.GetPatchedMethods();
        foreach (var method in myOriginalMethods) Mod.Logger.Event("Patched " + method.FullDescription());
    }

    public override void Dispose()
    {
        // don't run on server side
        if (Api == null) return;

        RenderGraph?.Invalidate();
        VanillaRenderGraph?.Dispose();
        _harmony?.UnpatchAll();
        ScreenManager.Platform.RebuildFrameBuffers();
        Api = null;
        Instance = null;
    }
}