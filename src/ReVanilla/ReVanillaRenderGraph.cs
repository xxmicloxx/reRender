using System;
using OpenTK.Graphics.OpenGL;
using ReRender.Extensions;
using ReRender.Graph;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace ReRender.ReVanilla;

public class VanillaRenderGraph : IDisposable
{
    private readonly ReRenderMod _mod;
    private readonly RenderGraph _renderGraph;
    private readonly WeatherSystemClient _weatherSystem;

    private readonly ChunkRenderer _chunkRenderer;
    private readonly EntityRenderer _entityRenderer;

    private ShaderProgram? _tonemapShader;
    private ShaderProgram? _lightingShader;
    private ShaderProgram? _ssaoShader;
    private ShaderProgram? _bilateralBlurShader;

    private TextureResourceType? _colorBufferTextureType;
    private TextureResourceType? _depthBufferTextureType;
    private TextureResourceType? _gBufferTextureType;
    private TextureResourceType? _ssaoTextureType;

    private readonly int _ssaoNoiseTexture;
    private readonly float[] _ssaoKernel = new float[192];
    
    private ITextureTarget? _primaryTextureTarget;

    public VanillaRenderGraph(ReRenderMod mod, RenderGraph renderGraph)
    {
        _mod = mod;
        _renderGraph = renderGraph;
        _weatherSystem = mod.Api!.ModLoader.GetModSystem<WeatherSystemClient>();

        _chunkRenderer = new ChunkRenderer(mod);
        _entityRenderer = new EntityRenderer(mod);
        
        _ssaoNoiseTexture = GL.GenTexture();
        InitSsao();
        
        _renderGraph.Updating += OnRenderGraphUpdate;
        _mod.Api!.Event.ReloadShader += LoadShaders;
    }

    private void InitSsao()
    {
        var rand = new Random();
        
        const int size = 16;
        var vecs = new float[size * size * 3];
        var tmpVec = new Vec3f();
        for (var i = 0; i < size * size; ++i)
        {
            tmpVec.Set((float)rand.NextDouble() * 2f - 1f, (float)rand.NextDouble() * 2f - 1f, 0f).Normalize();
            vecs[i * 3] = tmpVec.X;
            vecs[i * 3 + 1] = tmpVec.Y;
            vecs[i * 3 + 2] = tmpVec.Z;
        }
        
        GL.BindTexture(TextureTarget.Texture2D, _ssaoNoiseTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, size, size,
            0, PixelFormat.Rgb, PixelType.Float, vecs);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 
            (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);

        for (var i = 0; i < 64; ++i)
        {
            tmpVec.Set((float)rand.NextDouble() * 2f - 1f, (float)rand.NextDouble() * 2f - 1f,
                (float)rand.NextDouble());

            tmpVec.Normalize();
            tmpVec *= (float)rand.NextDouble();
            var scale = i / 64f;
            scale = GameMath.Lerp(0.1f, 1f, scale * scale);
            tmpVec *= scale;
            _ssaoKernel[i * 3] = tmpVec.X;
            _ssaoKernel[i * 3 + 1] = tmpVec.Y;
            _ssaoKernel[i * 3 + 2] = tmpVec.Z;
        }
    }

    public void Dispose()
    {
        _mod.Api!.Event.ReloadShader -= LoadShaders;
        
        GL.DeleteTexture(_ssaoNoiseTexture);
        
        _chunkRenderer.Dispose();
        _entityRenderer.Dispose();

        _tonemapShader?.Dispose();
        _lightingShader?.Dispose();
        _ssaoShader?.Dispose();
        _bilateralBlurShader?.Dispose();

        _renderGraph.Updating -= OnRenderGraphUpdate;
    }

    private bool LoadShaders()
    {
        var success = true;
        
        _chunkRenderer.LoadShaders(ref success);
        _entityRenderer.LoadShaders(ref success);

        _tonemapShader?.Dispose();
        _lightingShader?.Dispose();
        _ssaoShader?.Dispose();
        _bilateralBlurShader?.Dispose();

        _tonemapShader = _mod.RegisterShader("revanilla_tonemap", ref success);
        _lightingShader = _mod.RegisterShader("revanilla_lighting", ref success);
        _ssaoShader = _mod.RegisterShader("revanilla_ssao", ref success);
        _bilateralBlurShader = _mod.RegisterShader("revanilla_bilateralblur", ref success);

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

        var ssaoSize = new Size2i(
            (int)(context.RenderSize.Width * 0.5),
            (int)(context.RenderSize.Height * 0.5)
        );
        
        _ssaoTextureType = new TextureResourceTypeBuilder(ssaoSize, PixelInternalFormat.R8)
        {
            Filtering = TextureMinFilter.Linear
        }.Build();

        var primaryFb = context.FrameBuffers[(int)EnumFrameBuffer.Primary]!;
        var primaryTextureId = primaryFb.ColorTextureIds[0];
        _primaryTextureTarget = new ExternalTextureTarget(primaryTextureId, context.RenderSize);
    }

    private void BuildMainGraph(RenderSubgraph target, UpdateContext c)
    {
        target.Tasks.Clear();
        
        var depthBuffer = _depthBufferTextureType!.CreateResource("Depth Buffer");
        var gBufferColor = _colorBufferTextureType!.CreateResource("Color Buffer");
        var gBufferNormal = _gBufferTextureType!.CreateResource("Normal Buffer");
        var gBufferLighting = _colorBufferTextureType!.CreateResource("Lighting Buffer");

        var deferred = new RasterRenderTask
        {
            Name = "Fill GBuffers",
            DepthTarget = depthBuffer.ToTextureTarget(),
            ColorTargets = new ITextureTarget[]
            {
                gBufferColor.ToTextureTarget(), gBufferNormal.ToTextureTarget(), gBufferLighting.ToTextureTarget()
            },
            RenderAction = dt =>
            {
                _weatherSystem.cloudRenderer.CloudTick(dt);
                VanillaEmulation.UpdateMissingUniforms(c);
                
                FillGBuffers(dt, c);
            }
        };
        target.Tasks.Add(deferred);

        var occlusionBuffer = _ssaoTextureType!.CreateResource("Occlusion Buffer");
        var occlusionTask = new RasterRenderTask
        {
            Name = "Calculate Occlusion (SSAO)",
            ColorTargets = new ITextureTarget[] { occlusionBuffer.ToTextureTarget() },
            AdditionalResources = new Resource[] { depthBuffer, gBufferNormal },
            RenderAction = _ =>
            {
                CalculateOcclusion(c, depthBuffer, gBufferNormal);
            }
        };
        target.Tasks.Add(occlusionTask);

        var ssaoSource = occlusionBuffer;
        for (var i = 0; i < 1; ++i)
        {
            var blurHorTarget = _ssaoTextureType.CreateResource("Blur Buffer Horizontal");
            var blurHorTask =
                CreateBilateralBlurTask(c, blurHorTarget.ToTextureTarget(), ssaoSource, depthBuffer, false);
            target.Tasks.Add(blurHorTask);

            var blurVerTarget = _ssaoTextureType.CreateResource("Blur Buffer Vertical");
            var blurVerTask =
                CreateBilateralBlurTask(c, blurVerTarget.ToTextureTarget(), blurHorTarget, depthBuffer, true);
            target.Tasks.Add(blurVerTask);

            ssaoSource = blurVerTarget;
        }

        var lightingOutput = _gBufferTextureType!.CreateResource("Lighting Output Buffer");
        var lighting = new RasterRenderTask
        {
            Name = "Light scene",
            ColorTargets = new ITextureTarget[] { lightingOutput.ToTextureTarget() },
            AdditionalResources = new Resource[]
                { depthBuffer, gBufferColor, gBufferNormal, gBufferLighting, ssaoSource },
            RenderAction = _ =>
            {
                CalculateLighting(c, depthBuffer, gBufferColor, gBufferNormal, gBufferLighting, ssaoSource);
            }
        };
        target.Tasks.Add(lighting);

        var output = new RasterRenderTask
        {
            Name = "Tonemap",
            ColorTargets = new[] { _primaryTextureTarget! },
            AdditionalResources = new Resource[] { lightingOutput },
            RenderAction = _ =>
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
        
        //target.Tasks.Add(CreateBlitTask(c, _primaryTextureTarget!, gBufferNormal));
    }

    private RenderTask CreateBilateralBlurTask(UpdateContext c, ITextureTarget target, TextureResource source,
        TextureResource depth, bool vertical)
    {
        var orientationStr = vertical ? "Vertical" : "Horizontal";
        
        return new RasterRenderTask
        {
            Name = $"Bilateral Blur ({orientationStr})",
            ColorTargets = new [] { target },
            AdditionalResources = new Resource[] { source, depth },
            RenderAction = _ =>
            {
                c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);

                var s = _bilateralBlurShader!;
                using (s.Bind())
                {
                    s.BindTexture2D("t_input", source.Instance!.TextureId);
                    s.BindTexture2D("t_depth", depth.Instance!.TextureId);
                    s.Uniform("u_isVertical", vertical ? 1 : 0);
                    s.Uniform("u_frameSize", new Vec2f(_ssaoTextureType!.Width, _ssaoTextureType.Height));
                    _mod.RenderEngine!.DrawFullscreenPass();
                }
            }
        };
    }

    private RenderTask CreateBlitTask(UpdateContext c, ITextureTarget target, TextureResource source)
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
    
    private void CalculateOcclusion(UpdateContext c, TextureResource depthBuffer, TextureResource gBufferNormal)
    {
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 1f, 1f, 1f, 1f });
        c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);

        var s = _ssaoShader!;
        using (s.Bind())
        {
            c.BindKnownUniforms(s);
            s.BindTexture2D("t_depth", depthBuffer.Instance!.TextureId, 0);
            s.BindTexture2D("t_normal", gBufferNormal.Instance!.TextureId, 1);
            s.BindTexture2D("t_noise", _ssaoNoiseTexture, 2);
            s.Uniform("u_screenSize", new Vec2f(_ssaoTextureType!.Width, _ssaoTextureType.Height));
            s.Uniforms3("u_samples", 64, _ssaoKernel);
            _mod.RenderEngine!.DrawFullscreenPass();
        }
    }

    private void CalculateLighting(UpdateContext c, TextureResource depthBuffer, TextureResource gBufferColor,
        TextureResource gBufferNormal, TextureResource gBufferLighting, TextureResource ssaoSource)
    {
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 0.84f, 2.8f, 4f, 0f });

        c.SetupDraw(BlendMode.Disabled, DepthMode.Disabled, CullMode.Disabled);
        
        var s = _lightingShader!;
        using (s.Bind())
        {
            c.BindKnownUniforms(s);
            s.BindTexture2D("t_depth", depthBuffer.Instance!.TextureId, 0);
            s.BindTexture2D("t_color", gBufferColor.Instance!.TextureId, 1);
            s.BindTexture2D("t_normal", gBufferNormal.Instance!.TextureId, 2);
            s.BindTexture2D("t_lighting", gBufferLighting.Instance!.TextureId, 3);
            s.BindTexture2D("t_occlusion", ssaoSource.Instance!.TextureId, 4);
            _mod.RenderEngine!.DrawFullscreenPass();
        }
    }

    private void FillGBuffers(float dt, UpdateContext c)
    {
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.ClearBuffer(ClearBuffer.Color, 0, new[] { 0f, 0f, 0f, 0f });
        GL.ClearBuffer(ClearBuffer.Color, 1, new[] { 1f, 1f, 1f, 1f });
        GL.ClearBuffer(ClearBuffer.Color, 2, new[] { 0f, 0f, 0f, 0f });

        _chunkRenderer.Render(c);
        _entityRenderer.Render(dt, c);
    }
}