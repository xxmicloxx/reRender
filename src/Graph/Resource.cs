namespace ReRender.Graph;

public abstract class Resource
{
    public abstract ResourceType BaseResourceType { get; }
    public abstract ResourceInstance? BaseInstance { get; set; }
}

public abstract class Resource<TType, TInstance> : Resource
    where TType : ResourceType
    where TInstance : ResourceInstance<TType>
{
    protected Resource(TType resourceType)
    {
        ResourceType = resourceType;
    }

    public TType ResourceType { get; }
    public override ResourceType BaseResourceType => ResourceType;

    public override ResourceInstance? BaseInstance
    {
        get => Instance;
        set => Instance = (TInstance?)value;
    }

    public TInstance? Instance { get; set; }
}