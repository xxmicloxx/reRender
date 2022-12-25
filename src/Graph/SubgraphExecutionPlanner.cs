using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReRender.Graph;

public class SubgraphExecutionPlanner
{
    private readonly ISet<ResourceAllocation> _resourceAllocations;
    private readonly IDictionary<Resource, ResourceUsage> _resources;
    private readonly IList<ExecutionPlanStep> _steps;
    private readonly RenderSubgraph _subgraph;
    private readonly ISet<ResourceAllocation> _unusedAllocations;

    public SubgraphExecutionPlanner(RenderSubgraph subgraph)
    {
        _resourceAllocations = new HashSet<ResourceAllocation>();
        _resources = new Dictionary<Resource, ResourceUsage>();
        _steps = new List<ExecutionPlanStep>();
        _unusedAllocations = new HashSet<ResourceAllocation>();

        _subgraph = subgraph;
    }

    public SubgraphExecutionPlan Plan()
    {
        CreateSteps();
        CalculateResourceUsages();
        CalculateResourceAllocations();

        return new SubgraphExecutionPlan(new ReadOnlyCollection<ExecutionPlanStep>(_steps), _resources.Values,
            _resourceAllocations);
    }

    private void CreateSteps()
    {
        foreach (var task in _subgraph.Tasks)
        {
            var step = new ExecutionPlanStep(task);
            _steps.Add(step);

            foreach (var res in task.Resources)
            {
                if (!_resources.TryGetValue(res, out var usage))
                {
                    usage = new ResourceUsage(res, step);
                    _resources.Add(res, usage);
                }

                usage.StepGiven = step;
            }
        }
    }

    private void CalculateResourceUsages()
    {
        foreach (var usage in _resources.Values)
        {
            usage.StepTaken.TakenResources.Add(usage);
            usage.StepGiven.GivenResources.Add(usage);
        }
    }

    private void CalculateResourceAllocations()
    {
        foreach (var step in _steps)
        {
            foreach (var taken in step.TakenResources)
            {
                var alloc = TakeOrAllocateResource(taken.Resource.BaseResourceType);
                alloc.Usages.Add(taken);
                taken.Allocation = alloc;
            }

            foreach (var given in step.GivenResources) GiveResource(given.Allocation!);
        }
    }

    private ResourceAllocation TakeOrAllocateResource(ResourceType type)
    {
        var alloc = _unusedAllocations.FirstOrDefault(alloc => Equals(alloc.ResourceType, type));
        if (alloc != null)
        {
            _unusedAllocations.Remove(alloc);
            return alloc;
        }

        // no alloc available, create new
        alloc = new ResourceAllocation(type);
        _resourceAllocations.Add(alloc);
        return alloc;
    }

    private void GiveResource(ResourceAllocation alloc)
    {
        _unusedAllocations.Add(alloc);
    }
}