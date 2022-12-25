using System;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace ReRender;

public static class HelperExtensions
{
    private static readonly ShaderBinding ShaderBindObj = new();

    public static IShaderProgram RegisterShader(this ReRenderMod mod, string name, ref bool success)
    {
        mod.Api!.Render.CheckGlError("rerender-shader-pre");
        var shader = (ShaderProgram)mod.Api.Shader.NewShaderProgram();
        shader.AssetDomain = mod.Mod.Info.ModID;
        mod.Api!.Shader.RegisterFileShaderProgram(name, shader);
        if (!shader.Compile()) success = false;
        mod.Api.Render.CheckGlError("rerender-shader");
        return shader;
    }

    public static IDisposable Bind(this IShaderProgram shader)
    {
        ShaderBindObj.CurrentBinding = shader;
        return ShaderBindObj;
    }

    private class ShaderBinding : IDisposable
    {
        private IShaderProgram? _currentBinding;

        public IShaderProgram? CurrentBinding
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