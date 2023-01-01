using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using ReRender.Graph;
using Vintagestory.API.MathTools;

namespace ReRender.VintageGraph;

public class TextureResource : Resource<TextureResourceType, TextureResourceInstance>
{
    public TextureResource(TextureResourceType resourceType, string? name = null) : base(resourceType, name)
    {
    }
}

public class TextureResourceType : ResourceType
{
    public readonly IReadOnlyList<float>? BorderColor;
    public readonly TextureMinFilter Filtering;
    public readonly int Height;
    public readonly int Width;
    public readonly PixelInternalFormat InternalFormat;
    public readonly TextureWrapMode WrapMode;

    public TextureResourceType(TextureResourceTypeBuilder builder)
    {
        Width = builder.Width!.Value;
        Height = builder.Height!.Value;
        InternalFormat = builder.InternalFormat!.Value;
        Filtering = builder.Filtering;
        WrapMode = builder.WrapMode;
        BorderColor = builder.BorderColor;
    }

    public TextureResourceType(Size2i size, PixelInternalFormat internalFormat) : this(
        new TextureResourceTypeBuilder(size, internalFormat))
    {
    }

    public Resource CreateBaseResource(string? name = null)
    {
        return CreateResource(name);
    }

    public ResourceInstance CreateResourceInstance()
    {
        return new TextureResourceInstance(this);
    }

    public long? GetGpuSize()
    {
        return (long)InternalFormat.GetBytesPerPixel() * Width * Height;
    }

    public TextureResource CreateResource(string? name = null)
    {
        return new TextureResource(this, name);
    }

    protected bool Equals(TextureResourceType other)
    {
        return Filtering == other.Filtering && Height == other.Height && InternalFormat == other.InternalFormat &&
               Width == other.Width && WrapMode == other.WrapMode && Equals(BorderColor, other.BorderColor);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TextureResourceType)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)Filtering;
            hashCode = (hashCode * 397) ^ Height;
            hashCode = (hashCode * 397) ^ (int)InternalFormat;
            hashCode = (hashCode * 397) ^ Width;
            hashCode = (hashCode * 397) ^ (int)WrapMode;
            hashCode = (hashCode * 397) ^ (BorderColor != null ? BorderColor.GetHashCode() : 0);
            return hashCode;
        }
    }

    public override string ToString()
    {
        return $"{InternalFormat}, {Width}x{Height}, Filtering: {Filtering}, WrapMode: {WrapMode}";
    }
}

public class TextureResourceTypeBuilder
{
    // TODO border clamp value

    public TextureResourceTypeBuilder()
    {
    }

    public TextureResourceTypeBuilder(Size2i size, PixelInternalFormat internalFormat)
    {
        Size = size;
        InternalFormat = internalFormat;
    }

    public int? Width { get; set; }
    public int? Height { get; set; }

    public Size2i? Size
    {
        get => Width == null || Height == null ? null : new Size2i(Width.Value, Height.Value);
        set
        {
            if (value == null)
            {
                Width = null;
                Height = null;
            }
            else
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
    }

    public PixelInternalFormat? InternalFormat { get; set; }

    // this is theoretically a sampler thing, but currently I am too lazy to implement samplers...
    public TextureMinFilter Filtering { get; set; } = TextureMinFilter.Nearest;

    public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.ClampToEdge;

    public float[]? BorderColor { get; set; }

    public TextureResourceType Build()
    {
        return new TextureResourceType(this);
    }
}