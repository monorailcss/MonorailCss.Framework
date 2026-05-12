using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Rewrites Tailwind v4 build-time CSS function macros (<c>--alpha</c>, <c>--spacing</c>) into
/// portable CSS. Walks every <see cref="ProcessedClass"/>'s AST nodes; placement before
/// <see cref="ColorModifierStage"/> means a theme value with baked-in alpha
/// (<c>--color-divider: --alpha(...)</c>) gets converted to <c>color-mix(...)</c> first, so the
/// subsequent opacity modifier stage's <c>Contains("color-mix")</c> guard skips it rather than
/// double-stacking transparency.
/// </summary>
internal sealed class TailwindFunctionExpansionStage : IPipelineStage
{
    public string Name => "Tailwind Function Expansion";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        if (!context.Metadata.TryGetValue("processedClasses", out var classesObj)
            || classesObj is not List<ProcessedClass> processedClasses)
        {
            return nodes;
        }

        for (var i = 0; i < processedClasses.Count; i++)
        {
            var processedClass = processedClasses[i];
            var rewritten = TailwindFunctionExpander.ExpandTree(processedClass.AstNodes);
            if (!ReferenceEquals(rewritten, processedClass.AstNodes))
            {
                processedClasses[i] = processedClass with { AstNodes = rewritten };
            }
        }

        return nodes;
    }
}
