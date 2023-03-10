using System;
using OpenTK.Graphics.OpenGL;

namespace ReRender.VintageGraph;

public static class GlUtil
{
    public static PixelFormat GetPixelFormat(this PixelInternalFormat internalFormat)
    {
        switch (internalFormat)
        {
            case PixelInternalFormat.DepthComponent16:
            case PixelInternalFormat.DepthComponent24:
            case PixelInternalFormat.DepthComponent32:
                return PixelFormat.DepthComponent;

            case PixelInternalFormat.Depth24Stencil8:
                return PixelFormat.DepthStencil;

            case PixelInternalFormat.R8:
            case PixelInternalFormat.R8Snorm:
            case PixelInternalFormat.R16:
            case PixelInternalFormat.R16Snorm:
            case PixelInternalFormat.R16f:
            case PixelInternalFormat.R32f:
                return PixelFormat.Red;
            
            case PixelInternalFormat.R8i:
            case PixelInternalFormat.R8ui:
            case PixelInternalFormat.R16i:
            case PixelInternalFormat.R16ui:
            case PixelInternalFormat.R32i:
            case PixelInternalFormat.R32ui:
                return PixelFormat.RedInteger;
            
            case PixelInternalFormat.Rg8:
            case PixelInternalFormat.Rg8Snorm:
            case PixelInternalFormat.Rg16:
            case PixelInternalFormat.Rg16Snorm:
            case PixelInternalFormat.Rg16f:
            case PixelInternalFormat.Rg32f:
                return PixelFormat.Rg;
            
            case PixelInternalFormat.Rg8i:
            case PixelInternalFormat.Rg8ui:
            case PixelInternalFormat.Rg16i:
            case PixelInternalFormat.Rg16ui:
            case PixelInternalFormat.Rg32i:
            case PixelInternalFormat.Rg32ui:
                return PixelFormat.RgInteger;
            
            case PixelInternalFormat.Rgb8:
            case PixelInternalFormat.Rgb8Snorm:
            case PixelInternalFormat.Rgb16:
            case PixelInternalFormat.Rgb16Snorm:
            case PixelInternalFormat.Rgb16f:
            case PixelInternalFormat.Rgb32f:
                return PixelFormat.Rgb;

            case PixelInternalFormat.Rgb8i:
            case PixelInternalFormat.Rgb8ui:
            case PixelInternalFormat.Rgb16i:
            case PixelInternalFormat.Rgb16ui:
            case PixelInternalFormat.Rgb32i:
            case PixelInternalFormat.Rgb32ui:
                return PixelFormat.RgbInteger;

            case PixelInternalFormat.Rgba8:
            case PixelInternalFormat.Rgba8Snorm:
            case PixelInternalFormat.Rgba16:
            case PixelInternalFormat.Rgba16Snorm:
            case PixelInternalFormat.Rgba16f:
            case PixelInternalFormat.Rgba32f:
                return PixelFormat.Rgba;

            case PixelInternalFormat.Rgba8i:
            case PixelInternalFormat.Rgba8ui:
            case PixelInternalFormat.Rgba16i:
            case PixelInternalFormat.Rgba16ui:
            case PixelInternalFormat.Rgba32i:
            case PixelInternalFormat.Rgba32ui:
                return PixelFormat.RgbaInteger;

            default:
                throw new ArgumentOutOfRangeException(nameof(internalFormat), internalFormat, null);
        }
    }

    public static PixelType GetPixelType(this PixelInternalFormat internalFormat)
    {
        switch (internalFormat)
        {
            case PixelInternalFormat.R8:
            case PixelInternalFormat.Rg8:
            case PixelInternalFormat.Rgb8:
            case PixelInternalFormat.Rgba8:
            case PixelInternalFormat.R8ui:
            case PixelInternalFormat.Rg8ui:
            case PixelInternalFormat.Rgb8ui:
            case PixelInternalFormat.Rgba8ui:
                return PixelType.UnsignedByte;

            case PixelInternalFormat.R8i:
            case PixelInternalFormat.Rg8i:
            case PixelInternalFormat.Rgb8i:
            case PixelInternalFormat.Rgba8i:
            case PixelInternalFormat.R8Snorm:
            case PixelInternalFormat.Rg8Snorm:
            case PixelInternalFormat.Rgb8Snorm:
            case PixelInternalFormat.Rgba8Snorm:
                return PixelType.Byte;

            case PixelInternalFormat.R16:
            case PixelInternalFormat.Rg16:
            case PixelInternalFormat.Rgb16:
            case PixelInternalFormat.Rgba16:
            case PixelInternalFormat.R16ui:
            case PixelInternalFormat.Rg16ui:
            case PixelInternalFormat.Rgb16ui:
            case PixelInternalFormat.Rgba16ui:
            case PixelInternalFormat.DepthComponent16:
                return PixelType.UnsignedShort;

            case PixelInternalFormat.R16i:
            case PixelInternalFormat.Rg16i:
            case PixelInternalFormat.Rgb16i:
            case PixelInternalFormat.Rgba16i:
            case PixelInternalFormat.R16Snorm:
            case PixelInternalFormat.Rg16Snorm:
            case PixelInternalFormat.Rgb16Snorm:
            case PixelInternalFormat.Rgba16Snorm:
                return PixelType.Short;

            case PixelInternalFormat.R32ui:
            case PixelInternalFormat.Rg32ui:
            case PixelInternalFormat.Rgb32ui:
            case PixelInternalFormat.Rgba32ui:
            case PixelInternalFormat.DepthComponent24:
            case PixelInternalFormat.DepthComponent32:
                return PixelType.UnsignedInt;

            case PixelInternalFormat.R32i:
            case PixelInternalFormat.Rg32i:
            case PixelInternalFormat.Rgb32i:
            case PixelInternalFormat.Rgba32i:
                return PixelType.Int;

            case PixelInternalFormat.R16f:
            case PixelInternalFormat.Rg16f:
            case PixelInternalFormat.Rgb16f:
            case PixelInternalFormat.Rgba16f:
                return PixelType.HalfFloat;

            case PixelInternalFormat.R32f:
            case PixelInternalFormat.Rg32f:
            case PixelInternalFormat.Rgb32f:
            case PixelInternalFormat.Rgba32f:
                return PixelType.Float;

            case PixelInternalFormat.Depth24Stencil8:
                return PixelType.UnsignedInt248;

            default:
                throw new ArgumentOutOfRangeException(nameof(internalFormat), internalFormat, null);
        }
    }

    public static int GetBytesPerPixel(this PixelInternalFormat intFormat)
    {
        var format = intFormat.GetPixelFormat();
        var type = intFormat.GetPixelType();

        var components = format switch
        {
            PixelFormat.Red => 1,
            PixelFormat.Rgb => 3,
            PixelFormat.Rgba => 4,
            PixelFormat.Rg => 2,
            PixelFormat.RgInteger => 2,
            PixelFormat.DepthStencil => 1,
            PixelFormat.DepthComponent => 1,
            PixelFormat.RedInteger => 1,
            PixelFormat.RgbInteger => 3,
            PixelFormat.RgbaInteger => 4,
            _ => throw new ArgumentOutOfRangeException()
        };

        var componentSize = type switch
        {
            PixelType.Byte => 1,
            PixelType.UnsignedByte => 1,
            PixelType.Short => 2,
            PixelType.UnsignedShort => 2,
            PixelType.Int => 4,
            PixelType.UnsignedInt => 4,
            PixelType.Float => 4,
            PixelType.HalfFloat => 2,
            PixelType.UnsignedInt248 => 4,
            _ => throw new ArgumentOutOfRangeException()
        };

        return components * componentSize;
    }
}