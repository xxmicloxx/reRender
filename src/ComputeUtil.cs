using OpenTK.Graphics.OpenGL;
using ReRender.VintageGraph;

namespace ReRender;

public static class ComputeUtil
{
    public static int CalculateGlobalGroupSize(int localGroupSize, int totalSize)
    {
        var floorDiv = totalSize / localGroupSize;
        var add = totalSize % localGroupSize == 0 ? 0 : 1;
        return floorDiv + add;
    }

    public static void DispatchCompute(TextureResourceType tex, int localX, int localY)
    {
        DispatchCompute(tex.Width, tex.Height, 1, localX, localY, 1);
    }

    public static void DispatchCompute(int targetX, int targetY, int targetZ, int localX, int localY, int localZ)
    {
        var globalX = CalculateGlobalGroupSize(localX, targetX);
        var globalY = CalculateGlobalGroupSize(localY, targetY);
        var globalZ = CalculateGlobalGroupSize(localZ, targetZ);
        
        GL.DispatchCompute(globalX, globalY, globalZ);
    }
}