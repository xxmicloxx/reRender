using System;
using System.Collections.Generic;

namespace ReRender.Graph;

public class SubgraphResourcesAllocation : IDisposable
{
    private readonly IEnumerable<ResourceInstance> _resourceInstances;
    private readonly IEnumerable<Resource> _resources;

    public SubgraphResourcesAllocation(IEnumerable<Resource> resources, IEnumerable<ResourceInstance> instances)
    {
        _resources = resources;
        _resourceInstances = instances;
    }

    public void Dispose()
    {
        foreach (var res in _resources) res.BaseInstance = null;
        foreach (var instance in _resourceInstances) instance.Dispose();
    }
}