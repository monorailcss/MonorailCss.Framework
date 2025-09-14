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
        var result = ImmutableList.CreateBuilder<AstNode>();

        foreach (var node in nodes)
        {
            if (node is StyleRule styleRule)
            {
                var mergedDeclarations = _merger.ExtractAndMergeDeclarations(styleRule.Nodes, _strategy);
                result.Add(styleRule with { Nodes = mergedDeclarations });
            }
            else if (node is AtRule atRule)
            {
                var processedChildren = Process(atRule.Nodes, context);
                result.Add(atRule with { Nodes = processedChildren });
            }
            else
            {
                result.Add(node);
            }
        }

        return result.ToImmutable();
    }
}