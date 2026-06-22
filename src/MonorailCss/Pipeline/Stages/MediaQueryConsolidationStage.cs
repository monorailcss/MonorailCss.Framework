using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Consolidates multiple @media rules with identical query conditions into single blocks.
/// This optimization reduces CSS output size and improves browser parsing performance.
/// </summary>
internal class MediaQueryConsolidationStage : IPipelineStage
{
    public string Name => "Media Query Consolidation";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        if (nodes.IsEmpty)
        {
            return nodes;
        }

        // Process nodes at each level (including within layers)
        return ProcessNodeList(nodes);
    }

    private ImmutableList<AstNode> ProcessNodeList(ImmutableList<AstNode> nodes)
    {
        if (nodes.IsEmpty)
        {
            return nodes;
        }

        // Pass 1: group @media rules by normalized query and decide whether any rewrite is needed.
        // A rewrite is only needed when some query repeats (consolidation) or a non-media at-rule is
        // present (it gets recursed into). When neither holds — e.g. a flat list of utility rules
        // with at most one rule per breakpoint — we return the input untouched instead of rebuilding
        // the whole list through a SortedDictionary.
        Dictionary<string, List<AtRule>>? mediaGroups = null;
        var needsRewrite = false;

        foreach (var node in nodes)
        {
            if (node is AtRule { Name: "media" } media)
            {
                mediaGroups ??= new Dictionary<string, List<AtRule>>(StringComparer.Ordinal);
                var key = NormalizeMediaQuery(media.Params);
                if (!mediaGroups.TryGetValue(key, out var list))
                {
                    list = [];
                    mediaGroups[key] = list;
                }

                list.Add(media);
                if (list.Count > 1)
                {
                    needsRewrite = true;
                }
            }
            else if (node is AtRule)
            {
                // Non-media at-rule (e.g. a nested layer): recurse to consolidate inside it.
                needsRewrite = true;
            }
        }

        if (!needsRewrite)
        {
            return nodes;
        }

        // Pass 2: rebuild in original order. A multi-rule media group collapses into one rule emitted
        // at its first occurrence; later members are skipped. Singles and non-media nodes stay put.
        HashSet<string>? emitted = null;
        var result = ImmutableList.CreateBuilder<AstNode>();

        foreach (var node in nodes)
        {
            if (node is AtRule { Name: "media" } media)
            {
                var key = NormalizeMediaQuery(media.Params);
                var group = mediaGroups![key];
                if (group.Count == 1)
                {
                    result.Add(media);
                    continue;
                }

                emitted ??= new HashSet<string>(StringComparer.Ordinal);
                if (emitted.Add(key))
                {
                    result.Add(ConsolidateMediaRules(group, group[0].Params));
                }
            }
            else
            {
                result.Add(ProcessNode(node));
            }
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Processes a single node, recursively handling at-rules with nested content.
    /// </summary>
    private AstNode ProcessNode(AstNode node)
    {
        if (node is AtRule atRule && atRule.Name != "media")
        {
            // Recursively process nested content for layers and other at-rules
            var processedChildren = ProcessNodeList(atRule.Nodes);
            return atRule with { Nodes = processedChildren };
        }

        return node;
    }

    /// <summary>
    /// Consolidates multiple media rules with the same query into a single rule.
    /// </summary>
    private AtRule ConsolidateMediaRules(IEnumerable<AtRule> mediaRules, string mediaParams)
    {
        var consolidatedChildren = ImmutableList.CreateBuilder<AstNode>();

        foreach (var mediaRule in mediaRules)
        {
            consolidatedChildren.AddRange(mediaRule.Nodes);
        }

        return new AtRule("media", mediaParams, consolidatedChildren.ToImmutable());
    }

    /// <summary>
    /// Normalizes media query strings to ensure consistent comparison.
    /// Handles variations in whitespace and formatting.
    /// </summary>
    private string NormalizeMediaQuery(string mediaQuery)
    {
        // Normalize whitespace and parentheses
        return mediaQuery
            .Trim()
            .Replace(" (", "(")
            .Replace("( ", "(")
            .Replace(" )", ")")
            .Replace(") ", ")")
            .Replace("  ", " ");
    }
}