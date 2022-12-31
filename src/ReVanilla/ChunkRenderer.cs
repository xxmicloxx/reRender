using System;
using ReRender.Extensions;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.ReVanilla;

public class ChunkRenderer : IDisposable
{
    private readonly ReRenderMod _mod;
    
    private ShaderProgram? _chunkOpaqueShader;
    private ShaderProgram? _chunkTopSoilShader;
    private ShaderProgram? _flowersShader;

    public ChunkRenderer(ReRenderMod mod)
    {
        _mod = mod;
    }

    public void LoadShaders(ref bool success)
    {
        _chunkOpaqueShader?.Dispose();
        _chunkTopSoilShader?.Dispose();
        _flowersShader?.Dispose();
        
        _chunkOpaqueShader = _mod.RegisterShader("revanilla_chunkopaque", ref success);
        _chunkTopSoilShader = _mod.RegisterShader("revanilla_chunktopsoil", ref success);
        _flowersShader = _mod.RegisterShader("revanilla_flowers", ref success);
        
        _chunkTopSoilShader.SetCustomSampler("t_terrainLinear", true);
        _flowersShader.SetCustomSampler("t_terrainLinear", true);
    }

    public void Dispose()
    {
        _chunkOpaqueShader?.Dispose();
        _chunkTopSoilShader?.Dispose();
        _flowersShader?.Dispose();
    }
    
    public void Render(UpdateContext c)
    {
        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Enabled);
        c.PushModelViewMatrix(c.Game.MainCamera.CameraMatrixOrigin);

        var chunkRenderer = c.Game.ChunkRenderer;

        var camPos = c.Game.EntityPlayer!.CameraPos!;
        var texIds = chunkRenderer.TextureIds;

        using (_chunkOpaqueShader!.Bind())
        {
            var s = _chunkOpaqueShader!;
            c.BindKnownUniforms(s);
            s.Uniform("u_alphaTest", 0.001f);

            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.Opaque][l].Render(camPos, "u_origin");
            }
        }

        using (_chunkTopSoilShader!.Bind())
        {
            var s = _chunkTopSoilShader!;
            c.BindKnownUniforms(s);

            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.TopSoil][l].Render(camPos, "u_origin");
            }
        }

        using (_chunkOpaqueShader!.Bind())
        {
            c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Disabled);

            var s = _chunkOpaqueShader!;
            c.BindKnownUniforms(s);
            s.Uniform("u_alphaTest", 0.25f);

            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.BlendNoCull][l].Render(camPos, "u_origin");
            }

            s.Uniform("u_alphaTest", 0.42f);
            for (var l = 0; l < texIds.Length; ++l)
            {
                s.BindTexture2D("t_terrain", texIds[l]);
                s.BindTexture2D("t_terrainLinear", texIds[l]);
                chunkRenderer.PoolsByRenderPass[(int)EnumChunkRenderPass.OpaqueNoCull][l].Render(camPos, "u_origin");
            }
        }

        DrawInstancedObjects(c);

        c.PopModelViewMatrix();
    }

    private void DrawInstancedObjects(UpdateContext c)
    {
        var chunkRenderer = c.Game.ChunkRenderer;
        var texIds = chunkRenderer.TextureIds;
        
        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Enabled);

        var s = _flowersShader!;
        using (s.Bind())
        {
            PlantsRenderer.StartRenderInstanced(c, s, texIds[0]);
            foreach (var item in chunkRenderer.AllInstancedFlowers)
            {
                PlantsRenderer.RenderInstance(item!, c, s);
            }
        }
    }
}