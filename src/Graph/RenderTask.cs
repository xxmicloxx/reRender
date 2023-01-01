using System.Collections.Generic;

namespace ReRender.Graph;

public abstract class RenderTask
{
    public string Name { get; set; } = "Unnamed";
    public abstract IEnumerable<Resource> Resources { get; }
    public abstract void Execute(float dt);

    public virtual void Allocate()
    {
    }

    public virtual void Deallocate()
    {
    }
}