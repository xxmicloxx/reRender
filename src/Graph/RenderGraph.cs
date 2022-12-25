using System;
using System.Collections.Generic;
using ReRender.VintageGraph;
using ReRender.Wrapper;
using Vintagestory.API.Client;

namespace ReRender.Graph;

public class RenderGraph
{
    public RenderGraph()
    {
        Subgraphs = new Dictionary<SubgraphType, RenderSubgraph>
        {
            { SubgraphType.Main, new RenderSubgraph() }
        };
    }

    public IReadOnlyDictionary<SubgraphType, RenderSubgraph> Subgraphs { get; }
    public event Action<UpdateContext>? Updating;

    public void Reallocate()
    {
        foreach (var subgraph in Subgraphs.Values) subgraph.EnsurePlanned().AllocateResources();
    }

    public void Update(List<FrameBufferRef> framebuffers, ClientMainWrapper game)
    {
        var context = new UpdateContext(framebuffers, game);
        Updating?.Invoke(context);
    }

    public void Invalidate()
    {
        foreach (var subgraph in Subgraphs.Values) subgraph.Invalidate();
    }

    public void ExecuteSubgraph(SubgraphType stage)
    {
        var subgraph = Subgraphs[stage];
        subgraph.ExecutionPlan!.Execute();
    }
}