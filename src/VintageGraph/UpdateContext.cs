using System;
using System.Collections.Generic;
using ReRender.Wrapper;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.VintageGraph;

public class UpdateContext
{
    public readonly List<FrameBufferRef> FrameBuffers;
    public readonly ClientMainWrapper Game;
    public readonly ClientPlatformWindows Platform;
    public readonly Size2i RenderSize;
    public readonly float SSAALevel;
    public readonly Size2i WindowSize;

    public UpdateContext(List<FrameBufferRef> frameBuffers, ClientMainWrapper game)
    {
        FrameBuffers = frameBuffers;
        Game = game;
        Platform = (ClientPlatformWindows)ScreenManager.Platform;
        WindowSize = new Size2i(Platform.window.Width, Platform.window.Height);
        SSAALevel = ClientSettings.SSAA;
        RenderSize = new Size2i((int)(WindowSize.Width * SSAALevel), (int)(WindowSize.Height * SSAALevel));
    }

    public void SetupDraw(BlendMode blendMode, DepthMode depthMode, CullMode cullMode)
    {
        switch (blendMode)
        {
            case BlendMode.Disabled:
                Platform.GlToggleBlend(false);
                break;
            case BlendMode.Standard:
                Platform.GlToggleBlend(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null);
        }

        switch (depthMode)
        {
            case DepthMode.Disabled:
                Platform.GlDisableDepthTest();
                Platform.GlDepthMask(false);
                break;
            case DepthMode.ReadOnly:
                Platform.GlEnableDepthTest();
                Platform.GlDepthMask(false);
                break;
            case DepthMode.Enabled:
                Platform.GlEnableDepthTest();
                Platform.GlDepthMask(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(depthMode), depthMode, null);
        }

        if (cullMode == CullMode.Enabled)
            Platform.GlEnableCullFace();
        else
            Platform.GlDisableCullFace();
    }

    public void PushModelViewMatrix(double[] matrix)
    {
        Game.GlMatrixModeModelView();
        Game.GlPushMatrix();
        Game.GlLoadMatrix(matrix);
    }

    public void PopModelViewMatrix()
    {
        Game.GlMatrixModeModelView();
        Game.GlPopMatrix();
    }
}

public enum DepthMode
{
    Disabled,
    ReadOnly,
    Enabled
}

public enum CullMode
{
    Disabled,
    Enabled
}

public enum BlendMode
{
    Disabled,
    Standard
}