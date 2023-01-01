using ReRender.Graph;
using Vintagestory.API.Client;

namespace ReRender.Gui;

public class SubgraphDetailDialog : GuiDialog
{
    private readonly ReRenderMod _mod;
    private readonly SubgraphType _type;
    private readonly RenderSubgraph _subgraph;

    private readonly ResourcesView _resourcesView;
    private readonly GraphView _graphView;

    private int _selectedTab;

    public SubgraphDetailDialog(SubgraphType type, RenderSubgraph subgraph, ReRenderMod mod) : base(mod.Api!)
    {
        _mod = mod;
        _type = type;
        _subgraph = subgraph;

        _resourcesView = new ResourcesView(subgraph);
        _graphView = new GraphView(mod, subgraph);

        SetupDialog();
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        var bgBounds = ElementStdBounds.DialogBackground();

        var tabBounds = ElementBounds.Fixed(-100, GuiStyle.TitleBarHeight + 5, 100, 200);

        var composer = capi.Gui.CreateCompo("reRender_subgraphSelection", dialogBounds)
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