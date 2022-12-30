using ReRender.VintageGraph;
using Vintagestory.API.MathTools;

namespace ReRender.ReVanilla;

public static class VanillaEmulation
{
    public static void UpdateMissingUniforms(UpdateContext c)
    {
        var game = c.Game;
        var uniforms = game.ShaderUniforms;
        
        var moonPos = game.Calendar.MoonPosition;
        var sunPosRel = game.Calendar.SunPositionNormalized;
        uniforms.SunPosition3D = sunPosRel;

        var moonPosRel = moonPos.Clone().Normalize();
        var moonBrightness = game.Calendar.MoonLightStrength;
        var sunBrightness = game.Calendar.SunLightStrength;
        var t = GameMath.Clamp(50f * (moonBrightness - sunBrightness), 0f, 1f);
        
        uniforms.LightPosition3D.Set(
            GameMath.Lerp(sunPosRel.X, moonPosRel.X, t),
            GameMath.Lerp(sunPosRel.Y, moonPosRel.Y, t),
            GameMath.Lerp(sunPosRel.Z, moonPosRel.Z, t)
        );
    }
}