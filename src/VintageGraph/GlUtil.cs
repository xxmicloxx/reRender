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
            case PixelInternalFormat.Rgb8:
            case PixelInternalFormat.Rgba8:
            case PixelInternalFormat.Rgb8ui:
            case PixelInternalFormat.Rgba8ui:
                return PixelType.UnsignedByte;

            case PixelInternalFormat.Rgb8i:
            case PixelInternalFormat.Rgba8i:
            case PixelInternalFormat.Rgb8Snorm:
            case PixelInternalFormat.Rgba8Snorm:
                return PixelType.Byte;

            case PixelInternalFormat.Rgb16:
            case PixelInternalFormat.Rgba16:
            case PixelInternalFormat.Rgb16ui:
            case PixelInternalFormat.Rgba16ui:
            case PixelInternalFormat.DepthComponent16:
                return PixelType.UnsignedShort;

            case PixelInternalFormat.Rgb16i:
            case PixelInternalFormat.Rgba16i:
            case PixelInternalFormat.Rgb16Snorm:
            case PixelInternalFormat.Rgba16Snorm:
                return PixelType.Short;

            case PixelInternalFormat.Rgb32ui:
            case PixelInternalFormat.Rgba32ui:
            case PixelInternalFormat.DepthComponent24:
            case PixelInternalFormat.DepthComponent32:
                return PixelType.UnsignedInt;

            case PixelInternalFormat.Rgb32i:
            case PixelInternalFormat.Rgba32i:
                return PixelType.Int;

            case PixelInternalFormat.Rgb16f:
            case PixelInternalFormat.Rgba16f:
                return PixelType.HalfFloat;

            case PixelInternalFormat.Rgb32f:
            case PixelInternalFormat.Rgba32f:
                return PixelType.Float;

            case PixelInternalFormat.Depth24Stencil8:
                return PixelType.UnsignedInt248;

            default:
                throw new ArgumentOutOfRangeException(nameof(internalFormat), internalFormat, null);
        }
    }
}