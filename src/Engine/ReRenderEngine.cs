using System.Collections.Generic;
using ReRender.Graph;
using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Engine;

public class ReRenderEngine
{
    private readonly EngineCore _core;
    private readonly ReRenderMod _mod;
    private readonly RenderGraph _renderGraph;
    private MeshRef? _screenQuad;

    public ReRenderEngine(ReRenderMod mod, RenderGraph renderGraph)
    {
        _mod = mod;
        _renderGraph = renderGraph;
        _core = new EngineCore(renderGraph);
    }

    public void AfterFramebufferInit(List<FrameBufferRef> framebuffers, MeshRef screenQuad)
    {
        _screenQuad = screenQuad;

        _mod.Mod.Logger.Notification("Patching framebuffers...");
        _core.PatchFramebuffers(framebuffers);

        _mod.Mod.Logger.Notification("Patching framebuffers was successful, allocating required resources...");
        _core.RebuildRenderGraph(framebuffers);
    }

    public void RunMainRenderCycle(float dt, ClientMain instance)
    {
        _core.MainRenderCycle(dt, instance);
    }

    public void DrawFullscreenPass()
    {
        var platform = (ClientPlatformWindows)ScreenManager.Platform;
        platform.RenderFullscreenTriangle(_screenQuad);
    }
}