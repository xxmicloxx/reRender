using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Gui;

public class SubgraphSelectionDialog : GuiDialog
{
    private readonly ReRenderMod _mod;
    
    public SubgraphSelectionDialog(ReRenderMod mod) : base(mod.Api!)
    {
        _mod = mod;
        
        SetupDialog();
    }

    public override string? ToggleKeyCombinationCode => null;
    public override bool UnregisterOnClose => true;

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        var bgBounds = ElementStdBounds.DialogBackground();

        const int height = 25;
        const int padding = 5;
        var titleFont = CairoFont.WhiteSmallishText();
        var font = CairoFont.WhiteSmallText();
        var titleBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 300, height);
        var textBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + height + 10 + 3, 125, height);
        var exportButtonBounds = ElementBounds.Fixed(135, GuiStyle.TitleBarHeight + height + 10, 80, height);
        var detailButtonBounds = ElementBounds.Fixed(220, GuiStyle.TitleBarHeight + height + 10, 80, height);

        var composer = capi.Gui.CreateCompo("reRender_subgraphSelection", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("reRender", OnTitleBarCloseClicked)
            .BeginChildElements(bgBounds)
            .AddStaticText("Subgraphs:", titleFont, titleBounds);

        foreach (var entry in _mod.RenderGraph!.Subgraphs)
        {
            var type = entry.Key;
            var subgraph = entry.Value;

            composer.AddStaticText(type.ToString(), font, textBounds);

            composer.AddSmallButton("Export", () =>
            {
                GraphvizExporter.ExportToClipboard(subgraph, _mod);
                return true;
            }, exportButtonBounds);
            
            composer.AddSmallButton("Detail...", () =>
            {
                TryClose();
                
                var dialog = new SubgraphDetailDialog(type, subgraph, _mod);
                dialog.TryOpen();
                
                return true;
            }, detailButtonBounds);

            textBounds = textBounds.BelowCopy(fixedDeltaY: padding);
            detailButtonBounds = detailButtonBounds.BelowCopy(fixedDeltaY: padding);
            exportButtonBounds = exportButtonBounds.BelowCopy(fixedDeltaY: padding);
        }

        var reloadButtonBounds = ElementBounds.Fixed(0, detailButtonBounds.fixedY + 5, 300, 40);
        composer.AddButton("Reload all", OnReloadClicked, reloadButtonBounds, CairoFont.WhiteSmallishText());

        SingleComposer = composer.EndChildElements().Compose();
    }

    private bool OnReloadClicked()
    {
        // recreate all buffers
        ScreenManager.Platform.RebuildFrameBuffers();
        _mod.Api!.ShowChatMessage("Reloading all frame buffers and render graphs succeeded.");
        return true;
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }
}