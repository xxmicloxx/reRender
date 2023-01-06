using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using ReRender.Graph;
using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.VintageGraph;

public class RasterRenderTask : RenderTask
{
    public ITextureTarget? DepthTarget { get; set; }
    public IList<ITextureTarget> ColorTargets { get; set; } = new List<ITextureTarget>();

    public Action<float>? RenderAction { get; set; }
    private FrameBufferRef? FrameBuffer { get; set; }

    public IList<Resource> AdditionalResources { get; set; } = new List<Resource>();

    private IEnumerable<ITextureTarget> AllTextures => ColorTargets
        .Concat(new[] { DepthTarget }.OfType<ITextureTarget>());

    public override IEnumerable<Resource> Resources => AllTextures
        .OfType<ResourceTextureTarget>().Select(x => x.Resource)
        .Concat(AdditionalResources);

    public override void Execute(float dt)
    {
        GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, -1, Name);
        
        var platform = (ClientPlatformWindows)ScreenManager.Platform;
        
        platform.LoadFrameBuffer(FrameBuffer);
        RenderAction?.Invoke(dt);
        
        GL.PopDebugGroup();
    }

    public override void Allocate()
    {
        var platform = (ClientPlatformWindows)ScreenManager.Platform;
        var lastFrameBuffer = platform.CurrentFrameBuffer;

        var width = 0;
        var height = 0;

        var firstTexture = AllTextures.FirstOrDefault();
        if (firstTexture != null)
        {
            width = firstTexture.Width;
            height = firstTexture.Height;
        }

        // allocate a framebuffer
        FrameBuffer = new FrameBufferRef
        {
            FboId = GL.GenFramebuffer(),
            Width = width,
            Height = height,
            ColorTextureIds = new int[ColorTargets.Count]
        };
        GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, FrameBuffer.FboId, -1, Name);
        
        platform.CurrentFrameBuffer = FrameBuffer;

        if (DepthTarget != null)
        {
            if (DepthTarget.Width != width || DepthTarget.Height != height)
                throw new InvalidOperationException("Texture sizes don't match!");

            var textureId = DepthTarget.TextureId;
            FrameBuffer.DepthTextureId = textureId;

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, textureId, 0);
        }

        for (var i = 0; i < ColorTargets.Count; ++i)
        {
            var target = ColorTargets[i];
            if (target.Width != width || target.Height != height)
                throw new InvalidOperationException("Texture sizes don't match!");

            var textureId = target.TextureId;
            FrameBuffer.ColorTextureIds[i] = textureId;

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i,
                TextureTarget.Texture2D, textureId, 0);
        }

        var drawBuffers = new DrawBuffersEnum[ColorTargets.Count];
        for (var i = 0; i < ColorTargets.Count; ++i) drawBuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
        GL.DrawBuffers(ColorTargets.Count, drawBuffers);

        platform.CurrentFrameBuffer = lastFrameBuffer;
    }

    public override void Deallocate()
    {
        ScreenManager.Platform.DisposeFrameBuffer(FrameBuffer, false);
        FrameBuffer = null;
    }
}