using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using ReRender.Graph;
using ReRender.VintageGraph;
using ReRender.Wrapper;
using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using static OpenTK.Graphics.OpenGL.FramebufferAttachment;
using static OpenTK.Graphics.OpenGL.FramebufferTarget;
using static OpenTK.Graphics.OpenGL.PixelInternalFormat;
using static OpenTK.Graphics.OpenGL.PixelType;
using static OpenTK.Graphics.OpenGL.TextureTarget;

namespace ReRender.Engine;

public class EngineCore
{
    private readonly ClientMainWrapper _client;
    private readonly RenderGraph _renderGraph;
    private readonly CommonUniforms _uniforms;

    public EngineCore(RenderGraph renderGraph, CommonUniforms uniforms)
    {
        _renderGraph = renderGraph;
        _uniforms = uniforms;
        _client = new ClientMainWrapper();
    }

    public void MainRenderCycle(float dt, ClientMain instance)
    {
        _client.Client = instance;

        ScreenManager.FrameProfiler.Mark("mrl");
        _client.UpdateResize();
        _client.UpdateFreeMouse();
        _client.UpdateCameraYawPitch(dt);

        var shUniforms = _client.ShaderUniforms;
        var ambientManager = _client.AmbientManager;
        if (_client.EntityPlayer?.Pos != null)
        {
            shUniforms.FlagFogDensity = ambientManager.BlendedFlatFogDensity;
            shUniforms.FlatFogStartYPos = ambientManager.BlendedFlatFogYPosForShader;
        }

        if (!_client.IsPaused) _client.EventManager.TriggerGameTick(_client.InWorldElapsedMs, instance);
        ScreenManager.FrameProfiler.Mark("gametick");
        if (_client.LagSimulation) _client.Platform.ThreadSpinWait(10000000);
        shUniforms.Update(dt, _client.Api);
        shUniforms.ZNear = _client.MainCamera.ZNear;
        shUniforms.ZFar = _client.MainCamera.ZFar;
        _client.TriggerRenderStage(EnumRenderStage.Before, dt);
        _uniforms.Update();
        _client.Platform.GlEnableDepthTest();
        _client.Platform.GlDepthMask(true);
        if (ambientManager.ShadowQuality > 0 && ambientManager.DropShadowIntensity > 0.01)
        {
            _client.TriggerRenderStage(EnumRenderStage.ShadowFar, dt);
            _client.TriggerRenderStage(EnumRenderStage.ShadowFarDone, dt);
            if (ambientManager.ShadowQuality > 1)
            {
                _client.TriggerRenderStage(EnumRenderStage.ShadowNear, dt);
                _client.TriggerRenderStage(EnumRenderStage.ShadowNearDone, dt);
            }
        }

        _client.GlMatrixModeModelView();
        _client.GlLoadMatrix(_client.MainCamera.CameraMatrix);
        var pmat = _client.Api.Render.PMatrix.Top;
        var mvmat = _client.Api.Render.MvMatrix.Top;
        for (var i = 0; i < 16; i++)
        {
            _client.PerspectiveProjectionMat[i] = pmat[i];
            _client.PerspectiveViewMat[i] = mvmat[i];
        }

        var frustumCuller = _client.FrustumCuller;
        frustumCuller.CalcFrustumEquations(_client.Player.Entity.Pos.AsBlockPos, pmat, mvmat);
        frustumCuller.lod0BiasSq = ClientSettings.LodBias * ClientSettings.LodBias;
        frustumCuller.lod2BiasSq = ClientSettings.LodBiasFar * ClientSettings.LodBiasFar;
        _renderGraph.ExecuteSubgraph(SubgraphType.Main, dt);
        //_client.TriggerRenderStage(EnumRenderStage.Opaque, dt);
        /*if (_client.DoTransparentRenderPass)
        {
            ScreenManager.FrameProfiler.Mark("rendTransp-begin");
            _client.Platform.LoadFrameBuffer(EnumFrameBuffer.Transparent);
            ScreenManager.FrameProfiler.Mark("rendTransp-fbloaded");
            _client.Platform.ClearFrameBuffer(EnumFrameBuffer.Transparent);
            ScreenManager.FrameProfiler.Mark("rendTransp-bufscleared");
            _client.TriggerRenderStage(EnumRenderStage.OIT, dt);
            _client.Platform.UnloadFrameBuffer(EnumFrameBuffer.Transparent);
            ScreenManager.FrameProfiler.Mark("rendTranspDone");
            _client.Platform.MergeTransparentRenderPass();
            ScreenManager.FrameProfiler.Mark("mergeTranspPassDone");
        }*/

        _client.Platform.GlDepthMask(true);
        _client.Platform.GlEnableDepthTest();
        _client.Platform.GlCullFaceBack();
        _client.Platform.GlEnableCullFace();
        //_client.TriggerRenderStage(EnumRenderStage.AfterOIT, dt);
    }

    public void PatchFramebuffers(List<FrameBufferRef> framebuffers)
    {
        var platform = (ClientPlatformWindows)ScreenManager.Platform;
        platform.CurrentFrameBuffer = null;
        GL.DrawBuffer(DrawBufferMode.Back);

        var primaryBuffer = framebuffers[(int)EnumFrameBuffer.Primary];
        ScreenManager.Platform.DisposeFrameBuffer(primaryBuffer);
        primaryBuffer.FboId = GL.GenFramebuffer();
        primaryBuffer.DepthTextureId = 0;
        platform.CurrentFrameBuffer = primaryBuffer;

        // create a primary buffer for the game to render to
        primaryBuffer.ColorTextureIds = new[] { GL.GenTexture() };

        var filterMode = ClientSettings.SSAA <= 1f ? TextureMinFilter.Nearest : TextureMinFilter.Linear;
        
        GL.BindTexture(Texture2D, primaryBuffer.ColorTextureIds[0]);
        GL.TexImage2D(Texture2D, 0, Rgba8, primaryBuffer.Width, primaryBuffer.Height, 0, PixelFormat.Rgba,
            UnsignedShort, IntPtr.Zero);
        GL.TexParameter(Texture2D, TextureParameterName.TextureMinFilter, (int)filterMode);
        GL.TexParameter(Texture2D, TextureParameterName.TextureMagFilter, (int)filterMode);
        GL.FramebufferTexture2D(Framebuffer, ColorAttachment0, Texture2D, primaryBuffer.ColorTextureIds[0], 0);

        GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });
    }

    public void RebuildRenderGraph(List<FrameBufferRef> framebuffers)
    {
        _renderGraph.Invalidate();
        _renderGraph.Update(framebuffers, _client);
        _renderGraph.Reallocate();
    }

    public UpdateContext CreateUpdateContext()
    {
        var fbs = ScreenManager.Platform.FrameBuffers;
        return new UpdateContext(fbs, _client);
    }
}