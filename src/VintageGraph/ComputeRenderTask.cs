using System;
using System.Collections.Generic;
using ReRender.Graph;

namespace ReRender.VintageGraph;

public class ComputeRenderTask : RenderTask
{
    public IList<Resource> AdditionalResources { get; set; } = new List<Resource>();
    
    public Action<float>? RenderAction { get; set; }
    
    public override IEnumerable<Resource> Resources => AdditionalResources;
    public override void Execute(float dt)
    {
        RenderAction?.Invoke(dt);
    }
}