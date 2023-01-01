using System.Collections.Generic;
using ReRender.Graph;
using Vintagestory.API.Client;

namespace ReRender.Gui;

public class ResourcesView
{
    private readonly RenderSubgraph _subgraph;

    public ResourcesView(RenderSubgraph subgraph)
    {
        _subgraph = subgraph;
    }

    public void Compose(GuiComposer composer)
    {
        const int height = 20;
        const int spacing = 0;
        var textBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 1000, height);
        var font = CairoFont.WhiteSmallText();
        
        var allocationCounts = new Dictionary<ResourceType, int>();
        foreach (var allocation in _subgraph.EnsurePlanned().ResourceAllocations)
        {
            if (!allocationCounts.TryGetValue(allocation.ResourceType, out var value)) value = 0;
            allocationCounts[allocation.ResourceType] = value + 1;
        }

        long total = 0;
        var resources = 0;
        foreach (var alloc in allocationCounts)
        {
            resources += alloc.Value;
            var size = alloc.Key.GetGpuSize() * alloc.Value;
            if (size != null) total += size.Value;
            
            var sizeText = size == null ? "unknown" : $"{size.Value:N0} Bytes";
            composer.AddStaticText($"{alloc.Key}: {alloc.Value}x, {sizeText}", font, textBounds);

            textBounds = textBounds.BelowCopy(fixedDeltaY: spacing);
        }

        textBounds.fixedY += 10;
        composer.AddStaticText($"Total: {resources} Resources, {total:N0} Bytes", font, textBounds);
    }
}