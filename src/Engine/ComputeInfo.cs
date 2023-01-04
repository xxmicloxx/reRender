using OpenTK.Graphics.OpenGL4;
using Vintagestory.API.MathTools;

namespace ReRender.Engine;

public class ComputeInfo
{
    public readonly Vec3i MaxWorkGroupCount;
    public readonly Vec3i MaxWorkGroupSize;
    public readonly int MaxWorkGroupInvocations;

    public ComputeInfo()
    {
        var temp = new int[3];

        for (var i = 0; i < 3; ++i) GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, i, out temp[i]);
        MaxWorkGroupCount = new Vec3i(temp[0], temp[1], temp[2]);

        for (var i = 0; i < 3; ++i) GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, i, out temp[i]);
        MaxWorkGroupSize = new Vec3i(temp[0], temp[1], temp[2]);

        GL.GetInteger((GetPName)All.MaxComputeWorkGroupInvocations, out MaxWorkGroupInvocations);
    }
}