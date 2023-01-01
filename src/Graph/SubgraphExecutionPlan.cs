using System;
using System.Collections.Generic;
using System.Linq;

namespace ReRender.Graph;

public class SubgraphExecutionPlan : IDisposable
{
    public IEnumerable<ResourceAllocation> ResourceAllocations { get; }
    private readonly IEnumerable<ResourceUsage> _resources;

    private readonly IReadOnlyList<ExecutionPlanStep> _steps;
    private SubgraphResourcesAllocation? _currentAllocation;

    public SubgraphExecutionPlan(IReadOnlyList<ExecutionPlanStep> steps, IEnumerable<ResourceUsage> resources,
        IEnumerable<ResourceAllocation> allocations)
    {
        _steps = steps;
        _resources = resources;
        ResourceAllocations = allocations;
    }

    public void Dispose()
    {
        DeallocateResources();
    }

    public void Execute(float dt)
    {
        if (ResourceAllocations == null) throw new InvalidOperationException("Not allocated");

        foreach (var step in _steps) step.Execute(dt);
    }

    public void AllocateResources()
    {
        DeallocateResources();

        var instances = new HashSet<ResourceInstance>();
        foreach (var alloc in ResourceAllocations) instances.Add(alloc.Allocate());

        var resources = _resources.Select(x => x.Resource).ToArray();
        _currentAllocation = new SubgraphResourcesAllocation(resources, instances);

        foreach (var step in _steps) step.Allocate();
    }

    public void DeallocateResources()
    {
        foreach (var step in _steps) step.Deallocate();

        _currentAllocation?.Dispose();
        _currentAllocation = null;
    }
}