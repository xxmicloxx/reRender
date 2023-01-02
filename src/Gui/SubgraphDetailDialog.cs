using ReRender.Graph;
using Vintagestory.API.Client;
using Vintagestory.Client;

namespace ReRender.Gui;

public class SubgraphDetailDialog : GuiDialog
{
    private readonly SubgraphType _type;

    private readonly ResourcesView _resourcesView;
    private readonly GraphView _graphView;
    private readonly TexturesView _texturesView;

    private int _selectedTab;

    public SubgraphDetailDialog(SubgraphType type, RenderSubgraph subgraph, ReRenderMod mod) : base(mod.Api!)
    {
        _type = type;

        _resourcesView = new ResourcesView(subgraph);
        _graphView = new GraphView(mod, subgraph);
        _texturesView = new TexturesView(mod, subgraph);

        SetupDialog();
    }

    private void SetupDialog()
    {
        var alignment = _selectedTab == 2 ? EnumDialogArea.RightBottom : EnumDialogArea.CenterMiddle;
        var alignmentOffset = _selectedTab == 2 ? -GuiStyle.DialogToScreenPadding : 0;
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(alignment)
            .WithFixedAlignmentOffset(alignmentOffset, alignmentOffset);

        var bgBounds = ElementStdBounds.DialogBackground();

        var tabBounds = ElementBounds.Fixed(-100, GuiStyle.TitleBarHeight + 5, 100, 200);

        var composer = capi.Gui.CreateCompo("reRender_detailSelection", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar($"reRender [{_type.ToString()}]", OnTitleBarCloseClicked)
            .AddVerticalTabs(new GuiTab[]
            {
                new()
                {
                    DataInt = 0,
                    Name = "Resources"
                },
                new()
                {
                    DataInt = 1,
                    Name = "Graph"
                },
                new()
                {
                    DataInt = 2,
                    Name = "Textures"
                }
            }, tabBounds, OnTabSelected, "tabs")
            .BeginChildElements(bgBounds);

        switch (_selectedTab)
        {
            default:
                _resourcesView.Compose(composer);
                break;
            case 1:
                _graphView.Compose(composer);
                break;
            case 2:
                _texturesView.Compose(composer);
                break;
        }

        SingleComposer = composer.EndChildElements().Compose();
        SingleComposer.GetVerticalTab("tabs").activeElement = _selectedTab;
    }

    private void OnTabSelected(int id, GuiTab tab)
    {
        _selectedTab = id;
        SetupDialog();
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }

    public override void OnGuiClosed()
    {
        Dispose();
    }

    public override string? ToggleKeyCombinationCode => null;
    public override bool UnregisterOnClose => true;
}