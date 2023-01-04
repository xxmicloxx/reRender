using System;
using OpenTK.Graphics.OpenGL;
using ReRender.Engine;
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
    private ComputeShaderProgram? _luminanceHistogramShader;
    private ComputeShaderProgram? _histogramAverageShader;

    private TextureResourceType? _colorBufferTextureType;
    private TextureResourceType? _depthBufferTextureType;
    private TextureResourceType? _gBufferTextureType;
    private TextureResourceType? _ssaoTextureType;

    private readonly SSBO _histogramSSBO;
    private readonly SSBO _histogramAverageSSBO;
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
        
        _histogramSSBO = new SSBO();
        _histogramAverageSSBO = new SSBO();
        InitHistogram();

        _renderGraph.Updating += OnRenderGraphUpdate;
        _mod.Api!.Event.ReloadShader += LoadShaders;
    }

    private void InitHistogram()
    {
        var clearVal = 0u;
        _histogramSSBO.ReserveAndClear(1024, PixelInternalFormat.R32ui, PixelFormat.RedInteger, PixelType.UnsignedInt,
            ref clearVal, BufferUsageHint.StreamCopy);
        
        // single float, read every frame
        var initialAverage = 0f;
        _histogramAverageSSBO.ReserveAndClear(4, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float,
            ref initialAverage, BufferUsageHint.StreamRead);
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
        
        GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        
        GL.DeleteTexture(_ssaoNoiseTexture);
        _histogramSSBO.Dispose();
        _histogramAverageSSBO.Dispose();
        
        _chunkRenderer.Dispose();
        _entityRenderer.Dispose();

        _tonemapShader?.Dispose();
        _lightingShader?.Dispose();
        _ssaoShader?.Dispose();
        _bilateralBlurShader?.Dispose();
        _luminanceHistogramShader?.Dispose();
        _histogramAverageShader?.Dispose();

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
        _luminanceHistogramShader?.Dispose();
        _histogramAverageShader?.Dispose();

        _tonemapShader = _mod.RegisterShader("revanilla_tonemap", ref success);
        _lightingShader = _mod.RegisterShader("revanilla_lighting", ref success);
        _ssaoShader = _mod.RegisterShader("revanilla_ssao", ref success);
        _bilateralBlurShader = _mod.RegisterShader("revanilla_bilateralblur", ref success);
        _luminanceHistogramShader = _mod.RegisterComputeShader("revanilla_luminanceHistogram", ref success);
        _histogramAverageShader = _mod.RegisterComputeShader("revanilla_histogramAverage", ref success);

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

        var histogramAverage = new ComputeRenderTask
        {
            Name = "Average Histogram",
            RenderAction = dt =>
            {
                var s = _histogramAverageShader!;
                using (s.Bind())
                {
                    s.BindBuffer(0, _histogramSSBO);
                    s.BindBuffer(1, _histogramAverageSSBO);
                    s.Uniform("u_minLogLum", -12.0f);
                    s.Uniform("u_logLumRange", 14.0f);
                    s.Uniform("u_numPixels", _gBufferTextureType.Width * _gBufferTextureType.Height);
                    s.Uniform("u_timeCoeff", dt);
                    
                    // make sure all writes are finished
                    GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
                    GL.DispatchCompute(1, 1, 1);
                }
            }
        };
        target.Tasks.Add(histogramAverage);
        
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
                    s.BindBuffer(0, _histogramAverageSSBO);
                    
                    // ensure our histogram value is recent
                    GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
                    _mod.RenderEngine!.DrawFullscreenPass();
                }
            }
        };
        target.Tasks.Add(output);
        
        var histogram = new ComputeRenderTask
        {
            Name = "Gather Luminance Histogram",
            AdditionalResources = new Resource[] { lightingOutput },
            RenderAction = _ =>
            {
                var s = _luminanceHistogramShader!;
                using (s.Bind())
                {
                    s.BindImage2D("t_lighting", lightingOutput.Instance!.TextureId, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
                    s.BindBuffer(0, _histogramSSBO);
                    s.Uniform("u_minLogLum", -12.0f);
                    s.Uniform("u_inverseLogLumRange", 1/14.0f);
                    ComputeUtil.DispatchCompute(lightingOutput.ResourceType, 16, 16);
                }
            }
        };
        target.Tasks.Add(histogram);
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