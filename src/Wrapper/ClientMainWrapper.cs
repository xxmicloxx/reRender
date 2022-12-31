using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;

namespace ReRender.Wrapper;

public class ClientMainWrapper
{
    private static readonly Getter<DefaultShaderUniforms> ShUniformsGetter =
        CreateGetter<DefaultShaderUniforms>("shUniforms");

    private static readonly Getter<AmbientManager> AmbientManagerGetter =
        CreateGetter<AmbientManager>("AmbientManager");

    private static readonly Getter<bool> IsPausedGetter =
        CreateGetter<bool>("IsPaused");

    private static readonly Getter<ClientEventManager> EventManagerGetter =
        CreateGetter<ClientEventManager>("eventManager");

    private static readonly Getter<bool> LagSimulationGetter =
        CreateGetter<bool>("LagSimulation");

    private static readonly Getter<ClientPlatformAbstract> PlatformGetter =
        CreateGetter<ClientPlatformAbstract>("Platform");

    private static readonly Getter<ClientCoreAPI> ApiGetter =
        CreateGetter<ClientCoreAPI>("api");

    private static readonly Getter<PlayerCamera> MainCameraGetter =
        CreateGetter<PlayerCamera>("MainCamera");

    private static readonly Getter<FrustumCulling> FrustumCullingGetter =
        CreateGetter<FrustumCulling>("frustumCuller");

    private static readonly Getter<ClientPlayer> PlayerGetter =
        CreateGetter<ClientPlayer>("player");

    private static readonly Getter<bool> DoTransparentRenderPassGetter =
        CreateGetter<bool>("doTransparentRenderPass");

    private static readonly Getter<ChunkRenderer> ChunkRendererGetter =
        CreateGetter<ChunkRenderer>("chunkRenderer");

    private static readonly Getter<EntityTextureAtlasManager> EntityAtlasManagerGetter =
        CreateGetter<EntityTextureAtlasManager>("EntityAtlasManager");

    private static readonly Getter<Dictionary<Entity, EntityRenderer>> EntityRenderersGetter =
        CreateGetter<Dictionary<Entity, EntityRenderer>>("EntityRenderers");

    private readonly AmbientManagerWrapper _ambientManagerWrapper = new();
    private readonly ChunkRendererWrapper _chunkRendererWrapper = new();
    private readonly PlayerCameraWrapper _mainCameraWrapper = new();

    private ClientMain? _client;

    public Action UpdateResize { get; private set; } = null!;
    public Action UpdateFreeMouse { get; private set; } = null!;
    public Action<float> UpdateCameraYawPitch { get; private set; } = null!;
    public EntityPlayer? EntityPlayer => _client?.EntityPlayer;
    public DefaultShaderUniforms ShaderUniforms => ShUniformsGetter(_client!);

    public AmbientManagerWrapper AmbientManager
    {
        get
        {
            _ambientManagerWrapper.AmbientManager = AmbientManagerGetter(_client!);
            return _ambientManagerWrapper;
        }
    }

    public bool IsPaused => IsPausedGetter(_client!);
    public ClientEventManager EventManager => EventManagerGetter(_client!);
    public long InWorldElapsedMs => _client!.InWorldEllapsedMs;
    public bool LagSimulation => LagSimulationGetter(_client!);
    public ClientPlatformAbstract Platform => PlatformGetter(_client!);
    public ClientCoreAPI Api => ApiGetter(_client!);
    public double[] PerspectiveProjectionMat => _client!.PerspectiveProjectionMat;
    public double[] PerspectiveViewMat => _client!.PerspectiveViewMat;
    public FrustumCulling FrustumCuller => FrustumCullingGetter(_client!);
    public ClientPlayer Player => PlayerGetter(_client!);
    public bool DoTransparentRenderPass => DoTransparentRenderPassGetter(_client!);
    public float[] CurrentProjectionMatrix => _client!.CurrentProjectionMatrix;
    public float[] CurrentModelViewMatrix => _client!.CurrentModelViewMatrix;
    public IClientGameCalendar Calendar => _client!.Calendar;
    public ClientGameCalendar GameWorldCalendar => (ClientGameCalendar)Calendar;
    public EntityTextureAtlasManager EntityAtlasManager => EntityAtlasManagerGetter(_client!);
    public Dictionary<Entity, EntityRenderer> EntityRenderers => EntityRenderersGetter(_client!);

    public PlayerCameraWrapper MainCamera
    {
        get
        {
            _mainCameraWrapper.PlayerCamera = MainCameraGetter(_client!);
            return _mainCameraWrapper;
        }
    }

    public ChunkRendererWrapper ChunkRenderer
    {
        get
        {
            _chunkRendererWrapper.ChunkRenderer = ChunkRendererGetter(_client!);
            return _chunkRendererWrapper;
        }
    }

    public ClientMain Client
    {
        get => _client!;
        set
        {
            if (_client == value) return;
            _client = value;

            UpdateResize = (Action)Delegate.CreateDelegate(typeof(Action), value, "UpdateResize");
            UpdateFreeMouse = (Action)Delegate.CreateDelegate(typeof(Action), value, "UpdateFreeMouse");
            UpdateCameraYawPitch =
                (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), value, "UpdateCameraYawPitch");
        }
    }

    public void TriggerRenderStage(EnumRenderStage stage, float dt)
    {
        _client!.TriggerRenderStage(stage, dt);
    }

    public void GlMatrixModeModelView()
    {
        _client!.GlMatrixModeModelView();
    }

    public void GlMatrixModeProjection()
    {
        _client!.GlMatrixModeProjection();
    }

    public void GlLoadMatrix(double[] m)
    {
        _client!.GlLoadMatrix(m);
    }

    public void GlPopMatrix()
    {
        _client!.GlPopMatrix();
    }

    public void GlPushMatrix()
    {
        _client!.GlPushMatrix();
    }

    public void GlLoadIdentity()
    {
        _client!.GlLoadIdentity();
    }

    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<ClientMain, Getter<T>>(name);
    }

    private delegate TResult Getter<out TResult>(ClientMain arg);
}