using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline;

internal class Pipeline
{
    private readonly ImmutableList<IPipelineStage> _stages;

    public Pipeline(params IPipelineStage[] stages)
    {
        _stages = stages.ToImmutableList();
    }

    public ImmutableList<AstNode> Process(
        ImmutableList<AstNode> input,
        PipelineContext? context = null)
    {
        context ??= new PipelineContext();

        return _stages.Aggregate(
            input,
            (nodes, stage) => stage.Process(nodes, context));
    }
}