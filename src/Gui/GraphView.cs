using ReRender.Graph;
using Vintagestory.API.Client;

namespace ReRender.Gui;

public class GraphView
{
    private readonly ReRenderMod _mod;
    private readonly RenderSubgraph _subgraph;

    public GraphView(ReRenderMod mod, RenderSubgraph subgraph)
    {
        _mod = mod;
        _subgraph = subgraph;
    }

    public void Compose(GuiComposer composer)
    {
        var textBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 1000, 20);
        var buttonBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 30, 200, 40);

        composer.AddStaticText(
            "The render graph can be exported as Graphviz code to the clipboard by clicking the button below.", CairoFont.WhiteSmallText(), textBounds)
            .AddButton("Export to Clipboard", ExportToClipboard, buttonBounds, CairoFont.WhiteSmallishText());
    }

    private bool ExportToClipboard()
    {
        GraphvizExporter.ExportToClipboard(_subgraph, _mod);
        return true;
    }
}