using System;
using HarmonyLib;
using OpenTK.Graphics.OpenGL;
using ReRender.Engine;
using ReRender.Graph;
using ReRender.Gui;
using ReRender.ReVanilla;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender;

public class ReRenderMod : ModSystem
{
    private Harmony? _harmony;
    public static ReRenderMod? Instance { get; private set; }
    public ICoreClientAPI? Api { get; private set; }
    public RenderGraph? RenderGraph { get; private set; }
    public ReRenderEngine? RenderEngine { get; private set; }
    public VanillaRenderGraph? VanillaRenderGraph { get; private set; }

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }

    public override void StartPre(ICoreAPI api)
    {
        if (api is not ICoreClientAPI clientApi) return;
        
        Api = clientApi;
        Instance = this;

        RenderGraph = new RenderGraph();
        RenderEngine = new ReRenderEngine(this, RenderGraph);
        VanillaRenderGraph = new VanillaRenderGraph(this, RenderGraph);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        if (!CheckPlatform()) return;
        if (!CheckGlVersion()) return;
        
        PatchGame();

        api.RegisterCommand("rerender", "Opens the reRender configuration dialog", "", (_, _) =>
        {
            var dialog = new SubgraphSelectionDialog(this);
            dialog.TryOpen();
        });

        // recreate all buffers
        ScreenManager.Platform.RebuildFrameBuffers();
    }

    private bool CheckPlatform()
    {
        var os = Environment.OSVersion;
        if (os.Platform != PlatformID.MacOSX) return true;

        var dialog = new UnsupportedPlatformDialog(Api!);
        Api!.Event.LevelFinalize += () =>
        {
            dialog.TryOpen();
        };
        
        return false;
    }
    
    private bool CheckGlVersion()
    {
        var major = GL.GetInteger(GetPName.MajorVersion);
        var minor = GL.GetInteger(GetPName.MinorVersion);

        if (major > 4 || (major == 4 && minor >= 3)) return true;

        ClientSettings.GlContextVersion = "4.3";
        
        var dialog = new RestartRequiredDialog(Api!);
        Api!.Event.LevelFinalize += () =>
        {
            dialog.TryOpen();
        };

        return false;
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