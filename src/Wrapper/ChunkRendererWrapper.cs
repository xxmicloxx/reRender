using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace ReRender.Wrapper;

public class ChunkRendererWrapper
{
    private static readonly Getter<int[]> TextureIdsGetter = CreateGetter<int[]>("textureIds");
    private static readonly Getter<Vec2f> BlockTextureSizeGetter = CreateGetter<Vec2f>("blockTextureSize");

    private ChunkRenderer? _renderer;

    public int[] TextureIds => TextureIdsGetter(_renderer!);
    public Vec2f BlockTextureSize => BlockTextureSizeGetter(_renderer!);
    public MeshDataPoolManager[][] PoolsByRenderPass => _renderer!.poolsByRenderPass;
    public LinkedList<InstancedBlocksPool> AllInstancedGrass => _renderer!.allInstancedGrass;
    public LinkedList<InstancedBlocksPool> AllInstancedFlowers => _renderer!.allInstancedFlowers;

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