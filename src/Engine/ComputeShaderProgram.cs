using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Engine;

public class ComputeShaderProgram : IDisposable
{
    private static readonly ComputeShaderBinding Binding = new();
    private static ComputeShaderProgram? _currentProgram;

    public string? AssetDomain { get; set; }
    public string? PassName { get; set; }
    public int ProgramId { get; set; }

    private readonly Dictionary<string, int> _uniformLocations = new();
    private Shader? _computeShader;
    private bool _disposed;

    public bool LoadFromFile()
    {
        var assets = ScreenManager.Platform.AssetManager;
        const string ext = ".csh";

        var assetLoc = new AssetLocation(AssetDomain, "shaders/" + PassName + ext);
        var asset = assets.TryGet(assetLoc);
        if (asset == null) return false;

        // TODO include support
        _computeShader = new Shader(EnumShaderType.ComputeShader, asset.ToText(), PassName + ext);
        return true;
    }

    public bool Compile()
    {
        _uniformLocations.Clear();
        var uniformNames = new HashSet<string>();

        _computeShader!.EnsureVersionSupported();
        var ok = _computeShader!.Compile();
        CollectUniformNames(_computeShader.Code, uniformNames);
        ok &= CreateShaderProgram();

        foreach (var uniformName in uniformNames)
            _uniformLocations[uniformName] = GL.GetUniformLocation(ProgramId, uniformName);

        return ok;
    }

    public void Uniform(string uniformName, float val)
    {
        EnsureActive();
        GL.Uniform1(_uniformLocations[uniformName], val);
    }

    public void Uniform(string uniformName, int val)
    {
        EnsureActive();
        GL.Uniform1(_uniformLocations[uniformName], val);
    }

    public void Uniforms1(string name, int count, float[] values)
    {
        EnsureActive();
        GL.Uniform1(_uniformLocations[name], count, values);
    }

    public void Uniforms1(string name, int count, int[] values)
    {
        EnsureActive();
        GL.Uniform1(_uniformLocations[name], count, values);
    }

    public void Uniform(string uniformName, Vec2f val)
    {
        EnsureActive();
        GL.Uniform2(_uniformLocations[uniformName], val.X, val.Y);
    }

    public void Uniform(string uniformName, Vec2i val)
    {
        EnsureActive();
        GL.Uniform2(_uniformLocations[uniformName], val.X, val.Y);
    }

    public void Uniforms2(string name, int count, float[] values)
    {
        EnsureActive();
        GL.Uniform2(_uniformLocations[name], count, values);
    }

    public void Uniforms2(string name, int count, int[] values)
    {
        EnsureActive();
        GL.Uniform2(_uniformLocations[name], count, values);
    }

    public void Uniform(string uniformName, Vec3f val)
    {
        EnsureActive();
        GL.Uniform3(_uniformLocations[uniformName], val.X, val.Y, val.Z);
    }

    public void Uniform(string uniformName, Vec3i val)
    {
        EnsureActive();
        GL.Uniform3(_uniformLocations[uniformName], val.X, val.Y, val.Z);
    }

    public void Uniforms3(string name, int count, float[] values)
    {
        EnsureActive();
        GL.Uniform3(_uniformLocations[name], count, values);
    }

    public void Uniforms3(string name, int count, int[] values)
    {
        EnsureActive();
        GL.Uniform3(_uniformLocations[name], count, values);
    }

    public void Uniform(string uniformName, Vec4f val)
    {
        EnsureActive();
        GL.Uniform4(_uniformLocations[uniformName], val.X, val.Y, val.Z, val.W);
    }

    public void Uniform(string uniformName, Vec4i val)
    {
        EnsureActive();
        GL.Uniform4(_uniformLocations[uniformName], val.X, val.Y, val.Z, val.W);
    }

    public void Uniforms4(string name, int count, float[] values)
    {
        EnsureActive();
        GL.Uniform4(_uniformLocations[name], count, values);
    }

    public void Uniforms4(string name, int count, int[] values)
    {
        EnsureActive();
        GL.Uniform4(_uniformLocations[name], count, values);
    }

    public void UniformMatrix(string name, float[] matrix)
    {
        EnsureActive();
        GL.UniformMatrix4(_uniformLocations[name], 1, false, matrix);
    }

    public void BindImage2D(string imageName, int texId, int imgNum, TextureAccess access, SizedInternalFormat format)
    {
        EnsureActive();
        GL.Uniform1(_uniformLocations[imageName], imgNum);
        GL.BindImageTexture(imgNum, texId, 0, false, 0, access, format);
    }

    public void BindBuffer(int num, SSBO buffer)
    {
        EnsureActive();
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, num, buffer.Id);
    }

    public void Use()
    {
        if (_currentProgram != null && _currentProgram != this)
            throw new InvalidOperationException("Double shader use is not allowed or possible");

        if (_disposed) throw new InvalidOperationException("Trying to use a disposed shader");

        GL.UseProgram(ProgramId);
        _currentProgram = this;
    }

    public void Stop()
    {
        GL.UseProgram(0);
        _currentProgram = null;
    }

    public IDisposable Bind()
    {
        Binding.CurrentBinding = this;
        return Binding;
    }

    private void EnsureActive()
    {
        if (_currentProgram?.ProgramId != ProgramId)
            throw new InvalidOperationException("Can't set uniform on not active shader " + PassName + "!");
    }

    private static void CollectUniformNames(string code, ISet<string> list)
    {
        foreach (Match item in Regex.Matches(code,
                     "(\\s|\\r\\n)uniform\\s*(?<type>float|int|ivec2|ivec3|ivec4|vec2|vec3|vec4|image2D|mat3|mat4)\\s*(\\[[\\d\\w]+\\])?\\s*(?<var>[\\d\\w]+)",
                     RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
        {
            var varName = item.Groups["var"].Value;
            list.Add(varName);
        }
    }

    private bool CreateShaderProgram()
    {
        ProgramId = GL.CreateProgram();
        GL.AttachShader(ProgramId, _computeShader!.ShaderId);
        GL.LinkProgram(ProgramId);
        GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out var outVal);
        var logText = GL.GetProgramInfoLog(ProgramId);
        if (outVal == 1) return true;

        ScreenManager.Platform.Logger.Error($"Error linking compute shader {PassName}: {logText.TrimEnd()}");
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        GL.DetachShader(ProgramId, _computeShader!.ShaderId);
        GL.DeleteShader(_computeShader.ShaderId);
        GL.DeleteProgram(ProgramId);
    }

    private class ComputeShaderBinding : IDisposable
    {
        private ComputeShaderProgram? _currentBinding;

        public ComputeShaderProgram? CurrentBinding
        {
            get => _currentBinding;
            set
            {
                _currentBinding?.Stop();
                _currentBinding = value;
                _currentBinding?.Use();
            }
        }

        public void Dispose()
        {
            CurrentBinding = null;
        }
    }
}