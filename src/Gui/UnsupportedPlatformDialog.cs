using Vintagestory.API.Client;

namespace ReRender.Gui;

public class UnsupportedPlatformDialog : GuiDialog
{
    public UnsupportedPlatformDialog(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        var bgBounds = ElementStdBounds.DialogBackground();

        var titleFont = CairoFont.WhiteSmallishText();
        var titleTextBounds = ElementBounds.Fixed(0, 0, 300, titleFont.UnscaledFontsize);
        var bodyTextBounds = titleTextBounds.BelowCopy(fixedDeltaY: 10).WithFixedHeight(50);

        var okBounds = bodyTextBounds.BelowCopy(fixedDeltaY: 5).WithFixedHeight(40);
        
        SingleComposer = capi.Gui.CreateCompo("reRender_unsupportedPlatform", dialogBounds)
            .AddShadedDialogBG(bgBounds, false)
            .BeginChildElements(bgBounds)
            .AddStaticText("Unsupported Platform", titleFont, titleTextBounds)
            .AddStaticText("macOS is not supported by reRender. The mod has not been loaded.",
                CairoFont.WhiteSmallText(), bodyTextBounds)
            .AddButton("Close", OnOkPressed, okBounds, CairoFont.WhiteSmallishText())
            .EndChildElements()
            .Compose();
    }

    private bool OnOkPressed()
    {
        TryClose();
        return true;
    }

    public override string? ToggleKeyCombinationCode => null;
}