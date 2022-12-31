using ReRender.Wrapper;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ReRender.Extensions;

public static class EntityShapeRenderers
{
    private static readonly Getter<bool> IsSpectatorGetter = CreateGetter<bool>("isSpectator");
    private static readonly Getter<MeshRef?> MeshRefOpaqueGetter = CreateGetter<MeshRef?>("meshRefOpaque");
    private static readonly Getter<MeshRef?> MeshRefOitGetter = CreateGetter<MeshRef?>("meshRefOit");
    private static readonly Getter<Vec4f> LightRgbsGetter = CreateGetter<Vec4f>("lightrgbs");
    private static readonly Getter<int> SkipRenderJointIdGetter = CreateGetter<int>("skipRenderJointId");
    private static readonly Getter<int> SkipRenderJointId2Getter = CreateGetter<int>("skipRenderJointId2");

    public static bool IsSpectator(this EntityShapeRenderer renderer)
    {
        return IsSpectatorGetter(renderer);
    }

    public static MeshRef? GetMeshRefOpaque(this EntityShapeRenderer renderer)
    {
        return MeshRefOpaqueGetter(renderer);
    }

    public static MeshRef? GetMeshRefOit(this EntityShapeRenderer renderer)
    {
        return MeshRefOitGetter(renderer);
    }

    public static Vec4f GetLightRgbs(this EntityShapeRenderer renderer)
    {
        return LightRgbsGetter(renderer);
    }

    public static int GetSkipRenderJointId(this EntityShapeRenderer renderer)
    {
        return SkipRenderJointIdGetter(renderer);
    }

    public static int GetSkipRenderJointId2(this EntityShapeRenderer renderer)
    {
        return SkipRenderJointId2Getter(renderer);
    }
    
    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<EntityShapeRenderer, Getter<T>>(name);
    }

    private static Setter<T> CreateSetter<T>(string name)
    {
        return WrapperHelper.CreateSetter<EntityShapeRenderer, T, Setter<T>>(name);
    }
    
    private delegate TResult Getter<out TResult>(EntityShapeRenderer arg);
    private delegate void Setter<in TParam>(EntityShapeRenderer obj, TParam val);
}