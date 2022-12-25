using Vintagestory.Client.NoObf;

namespace ReRender.Wrapper;

public class PlayerCameraWrapper
{
    private static readonly Getter<float> ZNearGetter = CreateGetter<float>("ZNear");
    private static readonly Getter<float> ZFarGetter = CreateGetter<float>("ZFar");
    private static readonly Getter<double[]> CameraMatrixGetter = CreateGetter<double[]>("CameraMatrix");
    private static readonly Getter<double[]> CameraMatrixOriginGetter = CreateGetter<double[]>("CameraMatrixOrigin");

    private PlayerCamera? _camera;

    public float ZNear => ZNearGetter(_camera!);
    public float ZFar => ZFarGetter(_camera!);
    public double[] CameraMatrix => CameraMatrixGetter(_camera!);
    public double[] CameraMatrixOrigin => CameraMatrixOriginGetter(_camera!);

    public PlayerCamera PlayerCamera
    {
        get => _camera!;
        set
        {
            if (_camera == value) return;
            _camera = value;
        }
    }

    private static Getter<T> CreateGetter<T>(string name)
    {
        return WrapperHelper.CreateGetter<PlayerCamera, Getter<T>>(name);
    }

    private delegate TResult Getter<out TResult>(PlayerCamera arg);
}