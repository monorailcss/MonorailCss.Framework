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

        var mediaGroups = new Dictionary<string, List<(int Index, AtRule Rule)>>();
        var resultMap = new SortedDictionary<int, AstNode>();

        // Single pass: group nodes with their original indices
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (node is AtRule atRule && atRule.Name == "media")
            {
                var key = NormalizeMediaQuery(atRule.Params);
                if (!mediaGroups.TryGetValue(key, out var value))
                {
                    value = [];
                    mediaGroups[key] = value;
                }

                value.Add((i, atRule));
            }
            else
            {
                // Process non-media nodes (including layers and other at-rules)
                var processedNode = ProcessNode(node);
                resultMap[i] = processedNode;
            }
        }

        // Add consolidated media queries at first occurrence position
        foreach (var (_, group) in mediaGroups)
        {
            var firstIndex = group[0].Index;

            if (group.Count > 1)
            {
                // Consolidate multiple media queries into one
                var consolidatedRule = ConsolidateMediaRules(group.Select(g => g.Rule), group[0].Rule.Params);
                resultMap[firstIndex] = consolidatedRule;
            }
            else
            {
                // Single media query, keep as is
                resultMap[firstIndex] = group[0].Rule;
            }
        }

        return resultMap.Values.ToImmutableList();
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