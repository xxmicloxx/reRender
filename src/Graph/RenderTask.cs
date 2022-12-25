using System.Collections.Generic;

namespace ReRender.Graph;

public abstract class RenderTask
{
    public abstract IEnumerable<Resource> Resources { get; }
    public abstract void Execute();

    public virtual void Allocate()
    {
    }

    public virtual void Deallocate()
    {
    }
}