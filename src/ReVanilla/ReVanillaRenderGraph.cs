using System;
using OpenTK.Graphics.OpenGL;
using ReRender.Extensions;
using ReRender.Graph;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace ReRender.ReVanilla;

public class VanillaRenderGraph : IDisposable
{
    private readonly ReRenderMod _mod;
    private readonly RenderGraph _renderGraph;

    private ShaderProgram? _chunkOpaqueShader;
    private ShaderProgram? _chunkTopSoilShader;
    private ShaderProgram? _flowersShader;
    private ShaderProgram? _tonemapShader;
    private ShaderProgram? _lightingShader;

    private TextureResourceType? _colorBufferTextureType;
    private TextureResourceType? _depthBufferTextureType;
    private TextureResourceType? _gBufferTextureType;

    private ITextureTarget? _primaryTextureTarget;

    public VanillaRenderGraph(ReRenderMod mod, RenderGraph renderGraph)
    {
        _mod = mod;
        _renderGraph = renderGraph;

        _renderGraph.Updating += OnRenderGraphUpdate;
        _mod.Api!.Event.ReloadShader += LoadShaders;
    }

    public void Dispose()
    {
        _mod.Api!.Event.ReloadShader -= LoadShaders;
        
        _chunkOpaqueShader?.Dispose();
        _chunkTopSoilShader?.Dispose();
        _flowersShader?.Dispose();
        _tonemapShader?.Dispose();
        _lightingShader?.Dispose();

        _renderGraph.Updating -= OnRenderGraphUpdate;
    }

    private bool LoadShaders()
    {
        _chunkOpaqueShader?.Dispose();
        _chunkTopSoilShader?.Dispose();
        _flowersShader?.Dispose();
        _tonemapShader?.Dispose();
        _lightingShader?.Dispose();

        var success = true;
        _chunkOpaqueShader = _mod.RegisterShader("revanilla_chunkopaque", ref success);
        _chunkTopSoilShader = _mod.RegisterShader("revanilla_chunktopsoil", ref success);
        _flowersShader = _mod.RegisterShader("revanilla_flowers", ref success);
        _tonemapShader = _mod.RegisterShader("revanilla_tonemap", ref success);
        _lightingShader = _mod.RegisterShader("revanilla_lighting", ref success);
        
        _chunkTopSoilShader.SetCustomSampler("t_terrainLinear", true);
        _flowersShader.SetCustomSampler("t_terrainLinear", true);
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
        _colorBufferTextureType = new TextureResourceType(context.RenderSize, PixelInternalFormat.Rgba8);
        _gBufferTextureType = new TextureResourceTypeBuilder(context.RenderSize, PixelInternalFormat.Rgba32f)
        {
            WrapMode = TextureWrapMode.ClampToBorder,
            BorderColor = new[] { 1f, 1f, 1f, 1f }
        }.Build();

        var primaryFb = context.FrameBuffers[(int)EnumFrameBuffer.Primary]!;
        var primaryTextureId = primaryFb.ColorTextureIds[0];
        _primaryTextureTarget = new ExternalTextureTarget(primaryTextureId, context.RenderSize);
    }

    private void BuildMainGraph(RenderSubgraph target, UpdateContext c)
    {
        target.Tasks.Clear();
        
        var depthBuffer = _depthBufferTextureType!.CreateResource();
        var gBufferColor = _colorBufferTextureType!.CreateResource();
        var gBufferNormal = _gBufferTextureType!.CreateResource();
        var gBufferLighting = _colorBufferTextureType!.CreateResource();

        var deferred = new RasterRenderTask
        {
            DepthTarget = depthBuffer.ToTextureTarget(),
            ColorTargets = new ITextureTarget[]
            {
                gBufferColor.ToTextureTarget(), gBufferNormal.ToTextureTarget(), gBufferLighting.ToTextureTarget()
            },
            RenderAction = () =>
            {
                VanillaEmulation.UpdateMissingUniforms(c);
                FillGBuffers(c);
            }
        };
        target.Tasks.Add(deferred);
        
        var lightingOutput = _gBufferTextureType!.CreateResource();
        var lighting = new RasterRenderTask
        {
            ColorTargets = new ITextureTarget[] { lightingOutput.ToTextureTarget() },
            AdditionalResources = new Resource[] { depthBuffer, gBufferColor, gBufferNormal, gBufferLighting },
            RenderAction = () =>
            {
                CalculateLighting(c, depthBuffer, gBufferColor, gBufferNormal, gBufferLighting);
            }
        };
        target.Tasks.Add(lighting);

        var output = new RasterRenderTask
        {
            ColorTargets = new[] { _primaryTextureTarget! },
            AdditionalResources = new Resource[] { lightingOutput },
            RenderAction = () =>
            {
                c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);
                var s = _tonemapShader!;
                using (s.Bind())
                {
                    s.BindTexture2D("t_scene", lightingOutput.Instance!.TextureId);
                    _mod.RenderEngine!.DrawFullscreenPass();
                }
                
                GL.BindSampler(0, 0);
            }
        };
        target.Tasks.Add(output);
    }

    private void CalculateLighting(UpdateContext c, TextureResource depthBuffer, TextureResource gBufferColor,
        TextureResource gBufferNormal, TextureResource gBufferLighting)
    {
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 0.84f, 2.8f, 4f, 0f });

        c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);
        
        var s = _lightingShader!;
        using (s.Bind())
        {
            c.BindKnownUniforms(s);
            s.BindTexture2D("t_depth", depthBuffer.Instance!.TextureId);
            s.BindTexture2D("t_color", gBufferColor.Instance!.TextureId);
            s.BindTexture2D("t_normal", gBufferNormal.Instance!.TextureId);
            s.BindTexture2D("t_lighting", gBufferLighting.Instance!.TextureId);
            _mod.RenderEngine!.DrawFullscreenPass();
        }
    }

    private void FillGBuffers(UpdateContext c)
    {
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 0f, 0f, 0f, 0f });
        GL.ClearBuffer(ClearBuffer.Color, 1, new[] { 1f, 1f, 1f, 1f });
        GL.ClearBuffer(ClearBuffer.Color, 2, new[] { 0f, 0f, 0f, 0f });

        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Enabled);
        c.PushModelViewMatrix(c.Game.MainCamera.CameraMatrixOrigin);

        var chunkRenderer = c.Game.ChunkRenderer;

        var camPos = c.Game.EntityPlayer!.CameraPos!;
        var texIds = chunkRenderer.TextureIds;

        using (_chunkOpaqueShader!.Bind())
        {
            var s = _chunkOpaqueShader!;
            c.BindKnownUniforms(s);
            s.Uniform("u_alphaTest", 0.001f);

            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.Opaque][l].Render(camPos, "u_origin");
            }
        }

        using (_chunkTopSoilShader!.Bind())
        {
            var s = _chunkTopSoilShader!;
            c.BindKnownUniforms(s);

            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.TopSoil][l].Render(camPos, "u_origin");
            }
        }

        using (_chunkOpaqueShader!.Bind())
        {
            c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Disabled);
            
            var s = _chunkOpaqueShader!;
            c.BindKnownUniforms(s);
            s.Uniform("u_alphaTest", 0.05f);
            
            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.BlendNoCull][l].Render(camPos, "u_origin");
            }
            
            s.Uniform("u_alphaTest", 0.15f);
            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.OpaqueNoCull][l].Render(camPos, "u_origin");
            }
        }

        DrawInstancedObjects(c);

        c.PopModelViewMatrix();
    }

    private void DrawInstancedObjects(UpdateContext c)
    {
        var chunkRenderer = c.Game.ChunkRenderer;
        var texIds = chunkRenderer.TextureIds;
        
        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Enabled);

        var s = _flowersShader!;
        using (s.Bind())
        {
            PlantsRenderer.StartRenderInstanced(c, s, texIds[0]);
            foreach (var item in chunkRenderer.AllInstancedFlowers)
            {
                PlantsRenderer.RenderInstance(item!, c, s);
            }
        }
    }
}