using System;
using ReRender.Extensions;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace ReRender.ReVanilla;

public class EntityRenderer : IDisposable
{
    private readonly ReRenderMod _mod;
    private ShaderProgram? _entityAnimatedShader;

    public EntityRenderer(ReRenderMod mod)
    {
        _mod = mod;
    }

    public void LoadShaders(ref bool success)
    {
        _entityAnimatedShader?.Dispose();

        _entityAnimatedShader = _mod.RegisterShader("revanilla_entityanimated", ref success);
    }

    public void Dispose()
    {
        _entityAnimatedShader?.Dispose();
    }

    public void Render(float dt, UpdateContext c)
    {
        c.SetupDraw(BlendMode.Disabled, DepthMode.Enabled, CullMode.Disabled);
        // TODO non-animated entities

        var s = _entityAnimatedShader!;
        using (s.Bind())
        {
            c.BindKnownUniforms(s);
            s.BindTexture2D("t_entity", c.Game.EntityAtlasManager.AtlasTextureIds[0]);
            s.Uniform("u_alphaTest", 0.5f);
            c.PushModelViewMatrix(c.Game.MainCamera.CameraMatrixOrigin);

            foreach (var pair in c.Game.EntityRenderers)
            {
                var entity = pair.Key;
                if (!entity.IsRendered || (entity == c.Game.EntityPlayer &&
                                           c.Game.Api.Render.CameraType == EnumCameraMode.FirstPerson &&
                                           !ClientSettings.ImmersiveFpMode)) continue;

                var renderer = pair.Value;
                if (renderer is EntityShapeRenderer shapeRenderer)
                {
                    RenderForShapeRenderer(shapeRenderer, s, dt, c);
                }

                // in all other cases, we cannot currently render the shape. Too bad :(
            }
            
            c.PopModelViewMatrix();
        }
    }

    private readonly Vec4f _renderColor = new();

    private void RenderForShapeRenderer(EntityShapeRenderer renderer, IShaderProgram s, float dt, UpdateContext c)
    {
        var meshRefOpaque = renderer.GetMeshRefOpaque();
        var meshRefOit = renderer.GetMeshRefOit();
        var entity = renderer.entity;

        if (renderer.IsSpectator() || (meshRefOpaque == null && meshRefOit == null)) return;

        renderer.frostAlpha += (renderer.targetFrostAlpha - renderer.frostAlpha) * dt / 2f;
        var fa = (float)Math.Round(GameMath.Clamp(renderer.frostAlpha, 0f, 1f), 4);

        s.Uniform("u_rgbaLight", renderer.GetLightRgbs());
        s.UniformMatrix("u_model", renderer.ModelMat);
        s.UniformMatrix("u_view", c.Game.CurrentModelViewMatrix);
        s.Uniform("u_additionalRenderFlags", renderer.AddRenderFlags);
        s.Uniform("u_warping_windWaveIntensity", (float)renderer.WindWaveIntensity);
        s.Uniform("u_skipRenderJointId", renderer.GetSkipRenderJointId());
        s.Uniform("u_skipRenderJointId2", renderer.GetSkipRenderJointId2());
        s.Uniform("u_entityId", (int)entity.EntityId);
        s.Uniform("u_frostAlpha", fa);

        _renderColor[0] = ((entity.RenderColor >> 16) & 0xFF) / 255f;
        _renderColor[1] = ((entity.RenderColor >> 8) & 0xFF) / 255f;
        _renderColor[2] = (entity.RenderColor & 0xFF) / 255f;
        _renderColor[3] = ((entity.RenderColor >> 24) & 0xFF) / 255f;

        s.Uniform("u_renderColor", _renderColor);
        s.UniformMatrices("u_elementTransforms", 36, entity.AnimManager.Animator.Matrices);

        if (meshRefOpaque != null) c.Platform.RenderMesh(meshRefOpaque);
        if (meshRefOit != null) c.Platform.RenderMesh(meshRefOit);
    }
}