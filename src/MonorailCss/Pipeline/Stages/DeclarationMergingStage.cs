using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline.Stages;

internal class DeclarationMergingStage : IPipelineStage
{
    private readonly DeclarationMerger _merger;
    private readonly MergeStrategy _strategy;

    public string Name => "Declaration Merging";

    public DeclarationMergingStage(DeclarationMerger merger, MergeStrategy strategy = MergeStrategy.LastWins)
    {
        _merger = merger;
        _strategy = strategy;
    }

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Fast path: merging is a structural no-op for a rule whose children are already distinct
        // single declarations (the overwhelmingly common utility shape, e.g. one property per rule).
        // Rebuild the list only once a node actually changes, so an all-no-op pass returns the input
        // unchanged — no 3000 throwaway StyleRule records, dictionaries, and immutable lists per call.
        ImmutableList<AstNode>.Builder? result = null;

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            AstNode replacement;

            if (node is StyleRule styleRule)
            {
                if (!NeedsMerge(styleRule.Nodes))
                {
                    replacement = node;
                }
                else
                {
                    var mergedDeclarations = _merger.ExtractAndMergeDeclarations(styleRule.Nodes, _strategy);
                    replacement = styleRule with { Nodes = mergedDeclarations };
                }
            }
            else if (node is AtRule atRule)
            {
                var processedChildren = Process(atRule.Nodes, context);
                replacement = ReferenceEquals(processedChildren, atRule.Nodes)
                    ? node
                    : atRule with { Nodes = processedChildren };
            }
            else
            {
                replacement = node;
            }

            if (result == null && !ReferenceEquals(replacement, node))
            {
                // First change: seed the builder with the unchanged prefix we skipped over.
                result = ImmutableList.CreateBuilder<AstNode>();
                for (var j = 0; j < i; j++)
                {
                    result.Add(nodes[j]);
                }
            }

            result?.Add(replacement);
        }

        return result?.ToImmutable() ?? nodes;
    }

    // True when extracting + merging would actually change the rule's children: a repeated property
    // (LastWins would collapse it) or a non-declaration child (which the merger hoists/reorders).
    // For the typical handful of distinct declarations this is a zero-allocation O(n^2) scan.
    private static bool NeedsMerge(ImmutableList<AstNode> nodes)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is not Declaration di)
            {
                return true;
            }

            for (var j = 0; j < i; j++)
            {
                if (nodes[j] is Declaration dj && string.Equals(dj.Property, di.Property, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}