using ReRender.Extensions;
using ReRender.VintageGraph;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace ReRender.ReVanilla;

public static class PlantsRenderer
{
    private static readonly float[] Mvp = new float[16];
    private static readonly Vec4f FPlane = new Vec4f();

    public static void StartRenderInstanced(UpdateContext c, ShaderProgram s, int texId)
    {
        c.BindKnownUniforms(s);
        s.BindTexture2D("t_terrain", texId);
        s.BindTexture2D("t_terrainLinear", texId);

        var p = c.Game.CurrentProjectionMatrix;
        // _mvp = p
        for (var j = 0; j < 16; j += 4)
        {
            for (var i = 0; i < 4; ++i)
            {
                Mvp[j + i] = p[j + i];
            }
        }

        var mv = c.Game.CurrentModelViewMatrix;
        // _mvp *= mv
        for (var l = 0; l < 4; ++l)
        {
            var p2 = Mvp[l];
            var p3 = Mvp[4 + l];
            var p4 = Mvp[8 + l];
            var p5 = Mvp[12 + l];
            for (var m = 0; m < 16; m += 4)
            {
                Mvp[m + l] = p2 * mv[m] + p3 * mv[m + 1] + p4 * mv[m + 2] + p5 * mv[m + 3];
            }
        }

        for (var k = 0; k < 4; ++k)
        {
            FPlane[k] = Mvp[k * 4 + 2] + Mvp[k * 4 + 3];
        }

        FPlane.NormalizeXYZ();
        FPlane[3] /= FPlane.LengthXYZ();

        s.Uniform("u_fplaneNear", FPlane);
        for (var n = 0; n < 4; n += 2)
        {
            for (var j = 0; j < 4; ++j)
            {
                FPlane[j] = (1 - (n & 2)) * Mvp[j * 4 + (n & 1)] + Mvp[j * 4 + 3];
            }

            FPlane.NormalizeXYZ();
            FPlane[3] /= FPlane.LengthXYZ();
            s.Uniform(n == 0 ? "u_fplaneL" : "u_fplaneR", FPlane);
        }

        s.Uniform("u_billboardDistSq", c.Game.FrustumCuller.ViewDistanceSq * 0.05f);
    }

    public static void RenderInstance(InstancedBlocksPool item, UpdateContext c, ShaderProgram s)
    {
        var buffer = item.GetBuffer();
        var newOrigin = item.GetNewOrigin();
        var modified = item.GetModified();
        var modelProvider = item.GetModelProvider();
        var colorMapBase = item.GetColorMapBase();
        var mesh = item.GetMesh();

        var instancesCount = buffer.CustomInts.Count;
        if (instancesCount <= 0 || newOrigin == null) return;

        s.Uniform("u_texSizeU", modelProvider.texSizeU);
        s.Uniform("u_texSizeV", modelProvider.texSizeV);
        s.Uniform("u_origin", newOrigin);
        s.Uniform("u_colormapBase", colorMapBase);

        if (modified) c.Game.Platform.GlDisableCullFace();

        s.Uniform("u_alphaTest", 0.05f);
        c.Game.Platform.RenderMeshInstanced(mesh, instancesCount);

        if (!modified) return;
        
        c.Game.Platform.GlEnableCullFace();
        item.SetModified(false);
    }
}