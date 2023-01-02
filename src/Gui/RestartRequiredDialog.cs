using System;
using System.Diagnostics;
using Vintagestory.API.Client;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;

namespace ReRender.Gui;

public class RestartRequiredDialog : GuiDialog
{
    public RestartRequiredDialog(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private void SetupDialog()
    {
        var os = Environment.OSVersion;
        var isRestartSupported = os.Platform == PlatformID.Win32NT;

        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        var bgBounds = ElementStdBounds.DialogBackground();

        var titleFont = CairoFont.WhiteSmallishText();
        var titleTextBounds = ElementBounds.Fixed(0, 0, 500, titleFont.UnscaledFontsize);
        var bodyTextBounds = titleTextBounds.BelowCopy(fixedDeltaY: 10).WithFixedHeight(100);

        var restartNowBounds = bodyTextBounds.BelowCopy(fixedDeltaY: 5).WithFixedSize(245, 40);
        var restartLaterBounds = restartNowBounds.RightCopy(10);

        SingleComposer = capi.Gui.CreateCompo("reRender_restartRequired", dialogBounds)
            .AddShadedDialogBG(bgBounds, false)
            .BeginChildElements(bgBounds)
            .AddStaticText("Vintage Story restart required", titleFont, titleTextBounds)
            .AddStaticText("reRender requires an OpenGL version of 4.3 or above. The game uses version 3.3 by " +
                           "default. This default has now automatically been changed, however, a game restart is " +
                           "required. Please select one of the options below. Please also note that reRender is " +
                           "disabled until you restart the game.", CairoFont.WhiteSmallText(), bodyTextBounds)
            .AddButton("Restart now", OnRestartNowClicked, restartNowBounds, CairoFont.WhiteSmallishText(),
                key: "restartNow")
            .AddButton("Restart later", OnRestartLaterClicked, restartLaterBounds, CairoFont.WhiteSmallishText())
            .AddIf(!isRestartSupported)
            .AddHoverText("Not supported on this platform. Please restart the game manually.",
                CairoFont.WhiteSmallText(), 250, restartNowBounds.FlatCopy())
            .EndIf()
            .EndChildElements()
            .Compose();

        if (!isRestartSupported)
        {
            var restartNow = SingleComposer.GetButton("restartNow");
            restartNow.Enabled = false;
        }
    }

    private bool OnRestartNowClicked()
    {
        ClientSettings.Inst.Save(force: true);


        var args = Environment.GetCommandLineArgs().RemoveEntry(0);
        Process.Start(Process.GetCurrentProcess().MainModule.FileName, string.Join(" ", args));
        
        // kill it with fire - we need to be fast
        Process.GetCurrentProcess().Kill();

        return true;
    }

    private bool OnRestartLaterClicked()
    {
        TryClose();
        return true;
    }

    public override string? ToggleKeyCombinationCode => null;
    public override bool UnregisterOnClose => true;
}