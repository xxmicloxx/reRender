using System.Collections.Generic;

namespace ReRender.Graph;

public class ResourceAllocation
{
    public ResourceAllocation(ResourceType type)
    {
        Usages = new HashSet<ResourceUsage>();
        ResourceType = type;
    }

    public ResourceType ResourceType { get; }
    public ISet<ResourceUsage> Usages { get; }

    public ResourceInstance Allocate()
    {
        var instance = ResourceType.CreateResourceInstance();

        foreach (var usage in Usages) usage.Resource.BaseInstance = instance;

        return instance;
    }
}