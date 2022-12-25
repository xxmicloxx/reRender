using Vintagestory.API.MathTools;

namespace ReRender.VintageGraph;

public class ExternalTextureTarget : ITextureTarget
{
    public ExternalTextureTarget(int textureId, int width, int height)
    {
        TextureId = textureId;
        Width = width;
        Height = height;
    }

    public ExternalTextureTarget(int textureId, Size2i size) : this(textureId, size.Width, size.Height)
    {
    }

    public Size2i Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public int TextureId { get; set; }
}