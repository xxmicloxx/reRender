using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace ReRender.Engine;

public class CommonUniforms
{
    private readonly ReRenderMod _mod;

    // ReSharper disable once InconsistentNaming
    private readonly Vec4f _tempVec4f = new();
    
    public readonly float[] InvProjectionMatrix = Mat4f.Create();
    public readonly float[] InvModelViewMatrix = Mat4f.Create();
    public readonly Vec4f CameraWorldPosition = new();
    public float DayLight { get; private set; }

    public CommonUniforms(ReRenderMod mod)
    {
        _mod = mod;
    }
    
    public void Update()
    {
        // before rendering: update our values
        Mat4f.Invert(InvProjectionMatrix, _mod.Api!.Render.CurrentProjectionMatrix);
        Mat4f.Invert(InvModelViewMatrix, _mod.Api!.Render.CameraMatrixOriginf);

        _tempVec4f.Set(0, 0, 0, 1);
        Mat4f.MulWithVec4(InvModelViewMatrix, _tempVec4f, CameraWorldPosition);
        
        DayLight = 1.25f * GameMath.Max(
            _mod.Api!.World.Calendar.DayLightStrength -
            _mod.Api!.World.Calendar.MoonLightStrength / 2f, 0.05f);
    }
}