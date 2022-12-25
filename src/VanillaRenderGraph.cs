using System;
using OpenTK.Graphics.OpenGL;
using ReRender.Graph;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace ReRender;

public class VanillaRenderGraph : IDisposable
{
    private readonly ReRenderMod _mod;
    private readonly RenderGraph _renderGraph;

    private IShaderProgram? _deferredChunkOpaque;
    private TextureResourceType? _depthBufferTextureType;

    private TextureResourceType? _gBufferTextureType;

    private ITextureTarget? _primaryTextureTarget;

    public VanillaRenderGraph(ReRenderMod mod, RenderGraph renderGraph)
    {
        _mod = mod;
        _renderGraph = renderGraph;

        _renderGraph.Updating += OnRenderGraphUpdate;
        _mod.Api!.Event.ReloadShader += LoadShaders;
        LoadShaders();
    }

    public void Dispose()
    {
        _mod.Api!.Event.ReloadShader -= LoadShaders;
        _deferredChunkOpaque?.Dispose();

        _renderGraph.Updating -= OnRenderGraphUpdate;
    }

    private bool LoadShaders()
    {
        var success = true;
        _deferredChunkOpaque = _mod.RegisterShader("deferred_chunkopaque", ref success);
        return success;
    }

    private void OnRenderGraphUpdate(UpdateContext context)
    {
        UpdateGlobals(context);
        BuildMainGraph(_renderGraph.Subgraphs[SubgraphType.Main], context);
    }

    private void UpdateGlobals(UpdateContext context)
    {
        _depthBufferTextureType = new TextureResourceType(context.RenderSize, PixelInternalFormat.DepthComponent32);
        _gBufferTextureType = new TextureResourceType(context.RenderSize, PixelInternalFormat.Rgba32f);

        var primaryFb = context.FrameBuffers[(int)EnumFrameBuffer.Primary]!;
        var primaryTextureId = primaryFb.ColorTextureIds[0];
        _primaryTextureTarget = new ExternalTextureTarget(primaryTextureId, context.RenderSize);
    }

    private void BuildMainGraph(RenderSubgraph target, UpdateContext c)
    {
        var depthBuffer = _depthBufferTextureType!.CreateResource();
        var gBufferColor = _gBufferTextureType!.CreateResource();

        var deferred = new RasterRenderTask
        {
            DepthTarget = depthBuffer.ToTextureTarget(),
            ColorTargets = new ITextureTarget[] { gBufferColor.ToTextureTarget() },
            RenderAction = () => { FillGBuffers(c); }
        };

        var output = new RasterRenderTask
        {
            ColorTargets = new[] { _primaryTextureTarget! },
            AdditionalResources = new Resource[] { gBufferColor },
            RenderAction = () =>
            {
                var blit = ShaderPrograms.Blit;
                blit.Use();
                blit.Scene2D = gBufferColor.Instance!.TextureId;
                _mod.RenderEngine!.DrawFullscreenPass();
                blit.Stop();
            }
        };

        target.Tasks.AddRange(new[] { deferred, output });
    }

    private void FillGBuffers(UpdateContext c)
    {
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 0f, 0f, 0f, 1f });

        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Enabled);
        c.PushModelViewMatrix(c.Game.MainCamera.CameraMatrixOrigin);

        var chunkRenderer = c.Game.ChunkRenderer;

        var camPos = c.Game.EntityPlayer!.CameraPos!;
        var texIds = chunkRenderer.TextureIds;

        using (_deferredChunkOpaque!.Bind())
        {
            _deferredChunkOpaque!.UniformMatrix("u_modelView", c.Game.CurrentModelViewMatrix);
            _deferredChunkOpaque.UniformMatrix("u_projection", c.Game.CurrentProjectionMatrix);

            for (var l = 0; l < texIds.Length; ++l)
            {
                _deferredChunkOpaque.BindTexture2D("u_terrainTex", texIds[l], 0);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.Opaque][l].Render(camPos, "u_origin");
            }
        }

        c.PopModelViewMatrix();
    }
}