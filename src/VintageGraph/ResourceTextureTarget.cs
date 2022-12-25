namespace ReRender.VintageGraph;

public class ResourceTextureTarget : ITextureTarget
{
    public readonly TextureResource Resource;

    public ResourceTextureTarget(TextureResource resource)
    {
        Resource = resource;
    }

    public int TextureId => Resource.Instance!.TextureId;
    public int Width => Resource.ResourceType.Width;
    public int Height => Resource.ResourceType.Height;
}

public static class ResourceTextureTargetConverter
{
    public static ResourceTextureTarget ToTextureTarget(this TextureResource resource)
    {
        return new ResourceTextureTarget(resource);
    }
}