using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ReRender.Graph;

namespace ReRender.Gui;

public sealed class GraphvizExporter
{
    private readonly RenderSubgraph _subgraph;
    private int _nextTaskId;
    private int _nextResourceId;
    private readonly Dictionary<Resource, string> _resourceDict;
    private readonly Dictionary<RenderTask, string> _taskDict;

    private GraphvizExporter(RenderSubgraph subgraph)
    {
        _subgraph = subgraph;
        _resourceDict = new Dictionary<Resource, string>();
        _taskDict = new Dictionary<RenderTask, string>();
    }

    public static string Export(RenderSubgraph subgraph)
    {
        return new GraphvizExporter(subgraph).DoExport();
    }

    public static void ExportToClipboard(RenderSubgraph subgraph, ReRenderMod mod)
    {
        var code = Export(subgraph);

        var thread = new Thread(() => Clipboard.SetText(code));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        mod.Api!.ShowChatMessage("Graphviz code was successfully copied to clipboard.");
    }
    
    private string GetOrReserveResourceName(Resource r, out bool wasFirst)
    {
        wasFirst = false;
        if (_resourceDict.TryGetValue(r, out var id)) return id;
        
        _resourceDict[r] = id = $"r{_nextResourceId++}";
        wasFirst = true;
        return id;
    }
    
    private string GetOrReserveTaskName(RenderTask t)
    {
        if (!_taskDict.TryGetValue(t, out var id))
        {
            _taskDict[t] = id = $"t{_nextTaskId++}";
        }

        return id;
    }

    private string DoExport()
    {
        var graph = new StringBuilder();
        graph.AppendLine("digraph G {");
        graph.AppendLine("  ordering=\"out\";");

        var lastTaskName = "start";
        foreach (var task in _subgraph.Tasks)
        {
            var taskName = GetOrReserveTaskName(task);
            graph.AppendLine($"  {lastTaskName} -> {taskName}");
            
            foreach (var resource in task.Resources)
            {
                var resourceName = GetOrReserveResourceName(resource, out var wasFirst);
                var constraintStr = !wasFirst ? ",constraint=false" : string.Empty;
                graph.AppendLine($"  {taskName} -> {resourceName} [arrowhead=none{constraintStr}];");
            }

            graph.AppendLine();
            lastTaskName = taskName;
        }

        graph.AppendLine("  start [height=0,width=0,label=\"\",shape=none];");
        foreach (var taskPair in _taskDict)
        {
            graph.AppendLine($"  {taskPair.Value} [shape=box,label=\"{taskPair.Key.Name}\",group=t];");
        }

        foreach (var resPair in _resourceDict)
        {
            graph.AppendLine($"  {resPair.Value} [label=\"{resPair.Key.Name}\",group=r];");
        }

        graph.AppendLine("}");
        return graph.ToString();
    }
}