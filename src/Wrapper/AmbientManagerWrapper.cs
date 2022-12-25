using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace ReRender.Wrapper;

public class AmbientManagerWrapper
{
    private static readonly Getter<float> DropShadowIntensityGetter = CreateGetter<float>("DropShadowIntensity");

    private AmbientManager? _manager;

    public float DropShadowIntensity => DropShadowIntensityGetter(_manager!);

    public AmbientManager AmbientManager
    {
        get => _manager!;
        set
        {
            if (_manager == value) return;
            _manager = value;
        }
    }

    public Vec4f BlendedFogColor
    {
        get => _manager!.BlendedFogColor;
        set => _manager!.BlendedFogColor = value;
    }

    public Vec3f BlendedAmbientColor
    {
        get => _manager!.BlendedAmbientColor;
        set => _manager!.BlendedAmbientColor = value;
    }

    public float BlendedFogDensity
    {
        get => _manager!.BlendedFogDensity;
        set => _manager!.BlendedFogDensity = value;
    }

    public float BlendedFogMin
    {
        get => _manager!.BlendedFogMin;
        set => _manager!.BlendedFogMin = value;
    }

    public float BlendedFlatFogDensity
    {
        get => _manager!.BlendedFlatFogDensity;
        set => _manager!.BlendedFlatFogDensity = value;
    }

    public float BlendedFlatFogYOffset
    {
        get => _manager!.BlendedFlatFogYOffset;
        set => _manager!.BlendedFlatFogYOffset = value;
    }

    public float BlendedCloudBrightness
    {
        get => _manager!.BlendedCloudBrightness;
        set => _manager!.BlendedCloudBrightness = value;
    }

    public float BlendedCloudDensity
    {
        get => _manager!.BlendedCloudDensity;
        set => _manager!.BlendedCloudDensity = value;
    }

    public float BlendedCloudYPos
    {
        get => _manager!.BlendedCloudYPos;
        set => _manager!.BlendedCloudYPos = value;
    }

    public float BlendedFlatFogYPosForShader
    {
        get => _manager!.BlendedFlatFogYPosForShader;
        set => _manager!.BlendedFlatFogYPosForShader = value;
    }

    public float BlendedSceneBrightness
    {
        get => _manager!.BlendedSceneBrightness;
        set => _manager!.BlendedSceneBrightness = value;
    }

    public float BlendedFogBrightness
    {
        get => _manager!.BlendedFogBrightness;
        set => _manager!.BlendedFogBrightness = value;
    }

    public int ShadowQuality
    {
        get => _manager!.ShadowQuality;
        set => _manager!.ShadowQuality = value;
    }

    public OrderedDictionary<string, AmbientModifier> CurrentModifiers => _manager!.CurrentModifiers;

    public float ViewDistance => _manager!.ViewDistance;

    public AmbientModifier Base => _manager!.Base;

    public void Init()
    {
        _manager!.Init();
    }

    public void SetFogRange(float density, float min)
    {
        _manager!.SetFogRange(density, min);
    }

    public void UpdateAmbient(float dt)
    {
        _manager!.UpdateAmbient(dt);
    }

    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<AmbientManager, Getter<T>>(name);
    }

    private delegate TResult Getter<out TResult>(AmbientManager arg);
}