using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace ReRender.Wrapper;

public class ChunkRendererWrapper
{
    private static readonly Getter<int[]> TextureIdsGetter = CreateGetter<int[]>("textureIds");

    private ChunkRenderer? _renderer;

    public int[] TextureIds => TextureIdsGetter(_renderer!);
    public MeshDataPoolManager[][] PoolsByRenderPass => _renderer!.poolsByRenderPass;

    public ChunkRenderer ChunkRenderer
    {
        get => _renderer!;
        set
        {
            if (_renderer == value) return;
            _renderer = value;
        }
    }

    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<ChunkRenderer, Getter<T>>(name);
    }

    private delegate TResult Getter<out TResult>(ChunkRenderer arg);
}