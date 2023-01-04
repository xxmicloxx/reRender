using System.Collections.Generic;
using ReRender.Extensions;
using ReRender.Graph;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Engine;

public class ReRenderEngine
{
    private readonly EngineCore _core;
    private readonly ReRenderMod _mod;
    private MeshRef? _screenQuad;

    public CommonUniforms Uniforms { get; }
    public ComputeInfo? ComputeInfo { get; private set; }

    public ReRenderEngine(ReRenderMod mod, RenderGraph renderGraph)
    {
        _mod = mod;
        Uniforms = new CommonUniforms(mod);
        _core = new EngineCore(renderGraph, Uniforms);
    }

    public void Init()
    {
        ComputeInfo = new ComputeInfo();
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
    
    public RenderTask CreateBlitTask(UpdateContext c, ITextureTarget target, TextureResource source)
    {
        return new RasterRenderTask
        {
            Name = "Blit",
            ColorTargets = new[] { target },
            AdditionalResources = new Resource[] { source },
            RenderAction = _ =>
            {
                c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);
                var s = ShaderPrograms.Blit;
                using (s.Bind())
                {
                    s.Scene2D = source.Instance!.TextureId;
                    _mod.RenderEngine!.DrawFullscreenPass();
                }
            }
        };
    }

    public UpdateContext CreateUpdateContext()
    {
        return _core.CreateUpdateContext();
    }
}