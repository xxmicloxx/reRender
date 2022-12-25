namespace ReRender.Graph;

public interface ResourceType
{
    public Resource CreateBaseResource();
    public ResourceInstance CreateResourceInstance();
}