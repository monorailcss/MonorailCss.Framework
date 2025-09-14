using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline.Stages;

internal class LayerAssignmentStage : IPipelineStage
{
    public string Name => "Layer Assignment";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        var themeNodes = ImmutableList.CreateBuilder<AstNode>();
        var baseNodes = ImmutableList.CreateBuilder<AstNode>();
        var componentNodes = ImmutableList.CreateBuilder<AstNode>();
        var utilityNodes = ImmutableList.CreateBuilder<AstNode>();

        foreach (var node in nodes)
        {
            if (ShouldGoToThemeLayer(node, context))
            {
                themeNodes.Add(node);
            }
            else if (ShouldGoToBaseLayer(node, context))
            {
                baseNodes.Add(node);
            }
            else if (ShouldGoToComponentLayer(node, context))
            {
                componentNodes.Add(node);
            }
            else
            {
                utilityNodes.Add(node);
            }
        }

        var result = ImmutableList.CreateBuilder<AstNode>();

        if (themeNodes.Count > 0)
        {
            result.Add(new AtRule("layer", "theme", themeNodes.ToImmutable()));
        }

        if (baseNodes.Count > 0)
        {
            result.Add(new AtRule("layer", "base", baseNodes.ToImmutable()));
        }

        if (componentNodes.Count > 0)
        {
            result.Add(new AtRule("layer", "components", componentNodes.ToImmutable()));
        }

        if (utilityNodes.Count > 0)
        {
            result.Add(new AtRule("layer", "utilities", utilityNodes.ToImmutable()));
        }

        return result.ToImmutable();
    }

    private bool ShouldGoToThemeLayer(AstNode node, PipelineContext context)
    {
        if (node is StyleRule styleRule)
        {
            return styleRule.Selector is ":root" or ":root, :host" or ":host";
        }

        if (context.Metadata.TryGetValue("layer", out var layer) && layer.ToString() == "theme")
        {
            return true;
        }

        return false;
    }

    private bool ShouldGoToBaseLayer(AstNode node, PipelineContext context)
    {
        if (node is RawCss)
        {
            return true;
        }

        if (context.Metadata.TryGetValue("layer", out var layer) &&
            layer.ToString() == "base")
        {
            return true;
        }

        return false;
    }

    private bool ShouldGoToComponentLayer(AstNode node, PipelineContext context)
    {
        if (context.Metadata.TryGetValue("layer", out var layer) &&
            layer.ToString() == "components")
        {
            return true;
        }

        if (node is StyleRule styleRule && styleRule.Selector.StartsWith('.'))
        {
            var selector = styleRule.Selector;
            if (!selector.Contains(':') && !selector.Contains(' ') &&
                !selector.Contains('>') && !selector.Contains('+') &&
                !selector.Contains('~') && !selector.Contains('['))
            {
                return context.Metadata.ContainsKey("isComponent");
            }
        }

        return false;
    }
}