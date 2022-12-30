using System;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Extensions;

public static class Shaders
{
    private static readonly ShaderBinding ShaderBindObj = new();

    public static ShaderProgram RegisterShader(this ReRenderMod mod, string name, ref bool success)
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

    public static void BindKnownUniforms(this UpdateContext c, ShaderProgram s)
    {
        var chunkRenderer = c.Game.ChunkRenderer;
        var uniforms = c.Game.ShaderUniforms;
        var myUniforms = ReRenderMod.Instance!.RenderEngine!.Uniforms;

        var shadowMapFar = ScreenManager.Platform.FrameBuffers[(int)EnumFrameBuffer.ShadowmapFar];
        var shadowMapNear = ScreenManager.Platform.FrameBuffers[(int)EnumFrameBuffer.ShadowmapNear];

        foreach (var uniformLoc in s.uniformLocations.Keys)
        {
            switch (uniformLoc)
            {
                case "u_modelView":
                    s.UniformMatrix(uniformLoc, c.Game.CurrentModelViewMatrix);
                    break;
                
                case "u_projection":
                    s.UniformMatrix(uniformLoc, c.Game.CurrentProjectionMatrix);
                    break;
                    
                case "u_blockTextureSize":
                    s.Uniform(uniformLoc, chunkRenderer.BlockTextureSize);
                    break;
                
                case "u_playerPos":
                    s.Uniform(uniformLoc, uniforms.PlayerPos);
                    break;
                
                case "u_invModelView":
                    s.UniformMatrix(uniformLoc, myUniforms.InvModelViewMatrix);
                    break;
                
                case "u_invProjection":
                    s.UniformMatrix(uniformLoc, myUniforms.InvProjectionMatrix);
                    break;
                
                case "u_lightDirection":
                    s.Uniform(uniformLoc, uniforms.LightPosition3D);
                    break;
                
                case "u_viewDistanceLod0":
                    s.Uniform(uniformLoc, ClientSettings.ViewDistance * ClientSettings.LodBias);
                    break;
                
                case "u_colormap_rects":
                    s.Uniforms4(uniformLoc, 160, uniforms.ColorMapRects4);
                    break;
                
                case "u_colormap_seasonRel":
                    s.Uniform(uniformLoc, uniforms.SeasonRel);
                    break;
                
                case "u_colormap_seaLevel":
                    s.Uniform(uniformLoc, uniforms.SeaLevel);
                    break;
                
                case "u_colormap_atlasHeight":
                    s.Uniform(uniformLoc, uniforms.BlockAtlasHeight);
                    break;
                
                case "u_colormap_seasonTemp":
                    s.Uniform(uniformLoc, uniforms.SeasonTemperature);
                    break;
                
                case "u_warping_windWaveIntensity":
                    s.Uniform(uniformLoc, uniforms.WindWaveIntensity);
                    break;
                
                case "u_warping_windSpeed":
                    s.Uniform(uniformLoc, uniforms.WindSpeed);
                    break;
                
                case "u_warping_windWaveCounter":
                    s.Uniform(uniformLoc, uniforms.WindWaveCounter);
                    break;
                
                case "u_warping_windWaveCounterHighFreq":
                    s.Uniform(uniformLoc, uniforms.WindWaveCounterHighFreq);
                    break;
                
                case "u_warping_waterWaveCounter":
                    s.Uniform(uniformLoc, uniforms.WaterWaveCounter);
                    break;
            }

            if (ShaderProgramBase.shadowmapQuality > 0)
            {
                switch (uniformLoc)
                {
                    case "u_shadow_rangeNear":
                        s.Uniform(uniformLoc, uniforms.ShadowRangeNear);
                        break;
                
                    case "u_shadow_rangeFar":
                        s.Uniform(uniformLoc, uniforms.ShadowRangeFar);
                        break;
                
                    case "u_shadow_toMapNear":
                        s.UniformMatrix(uniformLoc, uniforms.ToShadowMapSpaceMatrixNear);
                        break;
                
                    case "u_shadow_toMapFar":
                        s.UniformMatrix(uniformLoc, uniforms.ToShadowMapSpaceMatrixFar);
                        break;
                
                    case "u_shadow_mapWidthInv":
                        s.Uniform(uniformLoc, 1f / shadowMapFar!.Width);
                        break;
                
                    case "u_shadow_mapHeightInv":
                        s.Uniform(uniformLoc, 1f / shadowMapFar!.Height);
                        break;
                    
                    case "t_shadow_mapNear":
                        s.BindTexture2D(uniformLoc, shadowMapNear!.DepthTextureId);
                        break;
                    
                    case "t_shadow_mapFar":
                        s.BindTexture2D(uniformLoc, shadowMapFar!.DepthTextureId);
                        break;
                }
            }
        }
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