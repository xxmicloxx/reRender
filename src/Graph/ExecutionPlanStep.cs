using System.Collections.Generic;

namespace ReRender.Graph;

public class ExecutionPlanStep
{
    public ExecutionPlanStep(RenderTask task)
    {
        GivenResources = new List<ResourceUsage>();
        TakenResources = new List<ResourceUsage>();
        Task = task;
    }

    public IList<ResourceUsage> GivenResources { get; }
    public IList<ResourceUsage> TakenResources { get; }
    public RenderTask Task { get; }

    public void Execute(float dt)
    {
        Task.Execute(dt);
    }

    public void Allocate()
    {
        Task.Allocate();
    }

    public void Deallocate()
    {
        Task.Deallocate();
    }
}