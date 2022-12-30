using ReRender.Wrapper;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace ReRender.Extensions;

public static class InstancedBlocksPools
{
    private static readonly Getter<MeshData> BufferGetter = CreateGetter<MeshData>("buffer");
    private static readonly Getter<Vec3f?> NewOriginGetter = CreateGetter<Vec3f?>("newOrigin");
    private static readonly Getter<bool> ModifiedGetter = CreateGetter<bool>("modified");
    private static readonly Getter<IMeshCreator> ModelProviderGetter = CreateGetter<IMeshCreator>("modelProvider");
    private static readonly Getter<int> ColorMapBaseGetter = CreateGetter<int>("colorMapBase");
    private static readonly Getter<MeshRef> MeshGetter = CreateGetter<MeshRef>("mesh");

    private static readonly Setter<bool> ModifiedSetter = CreateSetter<bool>("modified");

    public static MeshData GetBuffer(this InstancedBlocksPool pool)
    {
        return BufferGetter(pool);
    }

    public static Vec3f? GetNewOrigin(this InstancedBlocksPool pool)
    {
        return NewOriginGetter(pool);
    }

    public static bool GetModified(this InstancedBlocksPool pool)
    {
        return ModifiedGetter(pool);
    }

    public static IMeshCreator GetModelProvider(this InstancedBlocksPool pool)
    {
        return ModelProviderGetter(pool);
    }

    public static int GetColorMapBase(this InstancedBlocksPool pool)
    {
        return ColorMapBaseGetter(pool);
    }

    public static MeshRef GetMesh(this InstancedBlocksPool pool)
    {
        return MeshGetter(pool);
    }

    public static void SetModified(this InstancedBlocksPool pool, bool modified)
    {
        ModifiedSetter(pool, modified);
    }
    
    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<InstancedBlocksPool, Getter<T>>(name);
    }

    private static Setter<T> CreateSetter<T>(string name)
    {
        return WrapperHelper.CreateSetter<InstancedBlocksPool, T, Setter<T>>(name);
    }

    private delegate TResult Getter<out TResult>(InstancedBlocksPool arg);
    private delegate void Setter<in TParam>(InstancedBlocksPool obj, TParam val);
}