using System.Collections.Generic;
using ReRender.Graph;
using ReRender.VintageGraph;
using Vintagestory.API.Client;
using Vintagestory.Client;

namespace ReRender.Gui;

public class TexturesView
{
    private readonly ReRenderMod _mod;
    private readonly RenderSubgraph _subgraph;
    private readonly Dictionary<TextureResource, string> _keyResourceMap;
    private TextureResource? _currentPreview;
    private RenderTask? _currentPreviewTask;
    private GuiComposer? _composer;

    public TexturesView(ReRenderMod mod, RenderSubgraph subgraph)
    {
        _mod = mod;
        _subgraph = subgraph;
        _keyResourceMap = new Dictionary<TextureResource, string>();
    }

    public void Compose(GuiComposer composer)
    {
        _composer = composer;

        _keyResourceMap.Clear();

        var textures = new Dictionary<TextureResource, int>();
        for (var i = 0; i < _subgraph.Tasks.Count; ++i)
        {
            var task = _subgraph.Tasks[i];
            foreach (var resource in task.Resources)
            {
                if (resource is not TextureResource tres) continue;
                if (textures.ContainsKey(tres)) continue;

                textures[tres] = i;
            }
        }

        const int height = 25;
        const int spacing = 5;
        var checkboxBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, height, height);
        var textBounds = ElementBounds.Fixed(height + 10, GuiStyle.TitleBarHeight + 3, 300, height);

        var id = 0;
        foreach (var texPair in textures)
        {
            var texture = texPair.Key;
            var taskId = texPair.Value;

            var key = $"switch_t{taskId}_r{id}";
            _keyResourceMap[texture] = key;

            composer.AddSwitch(on => { PreviewTexture(on ? texture : null); }, checkboxBounds, key, height);
            composer.AddStaticText($"{taskId:D2}: {texture.Name}", CairoFont.WhiteSmallText(), textBounds);

            textBounds = textBounds.BelowCopy(fixedDeltaY: spacing);
            checkboxBounds = checkboxBounds.BelowCopy(fixedDeltaY: spacing);
            ++id;
        }

        if (_currentPreview != null && _keyResourceMap.TryGetValue(_currentPreview, out var switchKey))
            composer.GetSwitch(switchKey).On = true;
    }

    private void UpdateSwitches()
    {
        if (_composer == null) return;

        foreach (var resPair in _keyResourceMap) _composer.GetSwitch(resPair.Value).On = _currentPreview == resPair.Key;
    }

    private void PreviewTexture(TextureResource? texture)
    {
        _subgraph.Invalidate();

        if (_currentPreview != null && _currentPreviewTask != null)
        {
            _subgraph.Tasks.Remove(_currentPreviewTask);
            _currentPreview = null;
            _currentPreviewTask = null;
        }

        _currentPreview = texture;

        if (_currentPreview != null)
        {
            var fbs = ScreenManager.Platform.FrameBuffers;
            var primaryFb = fbs[(int)EnumFrameBuffer.Primary];

            var primaryTarget =
                new ExternalTextureTarget(primaryFb.ColorTextureIds[0], primaryFb.Width, primaryFb.Height);

            var c = _mod.RenderEngine!.CreateUpdateContext();

            _currentPreviewTask = _mod.RenderEngine!.CreateBlitTask(c, primaryTarget, _currentPreview);
            _currentPreviewTask.Name = "Preview Blit";
            _subgraph.Tasks.Add(_currentPreviewTask);
        }

        _subgraph.Plan().AllocateResources();

        UpdateSwitches();
    }
}