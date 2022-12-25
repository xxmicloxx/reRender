using System.Collections.Generic;

namespace ReRender.Graph;

public class RenderSubgraph
{
    public RenderSubgraph()
    {
        Tasks = new List<RenderTask>();
    }

    public List<RenderTask> Tasks { get; }
    public SubgraphExecutionPlan? ExecutionPlan { get; private set; }

    public SubgraphExecutionPlan EnsurePlanned()
    {
        return ExecutionPlan ?? Plan();
    }

    public SubgraphExecutionPlan Plan()
    {
        return ExecutionPlan = new SubgraphExecutionPlanner(this).Plan();
    }

    public void Invalidate()
    {
        ExecutionPlan?.DeallocateResources();
        ExecutionPlan = null;
    }
}