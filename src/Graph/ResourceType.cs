namespace ReRender.Graph;

public interface ResourceType
{
    public Resource CreateBaseResource(string? name = null);
    public ResourceInstance CreateResourceInstance();
    public long? GetGpuSize();
}