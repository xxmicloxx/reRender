using System;
using OpenTK.Graphics.OpenGL;

namespace ReRender.VintageGraph;

// ReSharper disable once InconsistentNaming
public class SSBO : IDisposable
{
    // TODO maybe make this a resource in the future, but the benefit might be small
    public readonly int Id;
    private int _reservedSize;

    public SSBO()
    {
        Id = GL.GenBuffer();
        _reservedSize = -1;
    }

    public void Dispose()
    {
        GL.DeleteBuffer(Id);
    }

    public void Reserve(int size, BufferUsageHint hint)
    {
        Bind();
        if (_reservedSize != size) GL.BufferData(BufferTarget.ShaderStorageBuffer, size, IntPtr.Zero, hint);
        _reservedSize = size;
        Unbind();
    }

    public void ReserveAndClear<T>(int size, PixelInternalFormat internalFormat, PixelFormat format, PixelType type,
        ref T data, BufferUsageHint hint) where T : struct
    {
        Bind();
        if (_reservedSize != size) GL.BufferData(BufferTarget.ShaderStorageBuffer, size, IntPtr.Zero, hint);
        GL.ClearBufferData(BufferTarget.ShaderStorageBuffer, internalFormat, format, type, ref data);
        _reservedSize = size;
        Unbind();
    }
    
    public void ReserveAndClear<T>(int size, PixelInternalFormat internalFormat, PixelFormat format, PixelType type,
        T[] data, BufferUsageHint hint) where T : struct
    {
        Bind();
        if (_reservedSize != size) GL.BufferData(BufferTarget.ShaderStorageBuffer, size, IntPtr.Zero, hint);
        GL.ClearBufferData(BufferTarget.ShaderStorageBuffer, internalFormat, format, type, data);
        _reservedSize = size;
        Unbind();
    }

    public void Write<T>(int size, ref T data, BufferUsageHint hint) where T : struct
    {
        Bind();
        
        if (_reservedSize != size)
            GL.BufferData(BufferTarget.ShaderStorageBuffer, size, ref data, hint);
        else
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, size, ref data);
        
        _reservedSize = size;
        Unbind();
    }
    
    public void Write<T>(int size, T[] data, BufferUsageHint hint) where T: struct
    {
        Bind();
        
        if (_reservedSize != size)
            GL.BufferData(BufferTarget.ShaderStorageBuffer, size, data, hint);
        else
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, size, data);
        
        _reservedSize = size;
        Unbind();
    }

    public void Clear<T>(PixelInternalFormat internalFormat, PixelFormat format, PixelType type, ref T data) where T: struct
    {
        Bind();
        GL.ClearBufferData(BufferTarget.ShaderStorageBuffer, internalFormat, format, type, ref data);
        Unbind();
    }
    
    public void Clear<T>(PixelInternalFormat internalFormat, PixelFormat format, PixelType type, T[] data) where T: struct
    {
        Bind();
        GL.ClearBufferData(BufferTarget.ShaderStorageBuffer, internalFormat, format, type, data);
        Unbind();
    }

    public void Read<T>(int size, ref T data) where T: struct
    {
        Bind();
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, size, ref data);
        Unbind();
    }
    
    public void Read<T>(int size, T[] data) where T: struct
    {
        Bind();
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, size, data);
        Unbind();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
    }

    public static void Unbind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }
}