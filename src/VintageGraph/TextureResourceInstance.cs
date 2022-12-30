using System;
using OpenTK.Graphics.OpenGL;
using ReRender.Graph;

namespace ReRender.VintageGraph;

public class TextureResourceInstance : ResourceInstance<TextureResourceType>
{
    public readonly int TextureId;

    public TextureResourceInstance(TextureResourceType type)
    {
        ResourceType = type;

        TextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, TextureId);

        var format = type.InternalFormat;

        GL.TexImage2D(TextureTarget.Texture2D, 0, format, type.Width, type.Height, 0, format.GetPixelFormat(),
            format.GetPixelType(), IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)type.Filtering);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)type.Filtering);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)type.WrapMode);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)type.WrapMode);

        if (type.BorderColor != null)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor,
                (float[])type.BorderColor);
    }

    public override TextureResourceType ResourceType { get; }

    public override void Dispose()
    {
        GL.DeleteTexture(TextureId);
    }
}