using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Processing;
using MonorailCss.Sorting;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Combines post-processing and sorting in one stage to maintain proper ordering.
/// </summary>
internal class ProcessingAndSortingStage : IPipelineStage
{
    private readonly PostProcessor _postProcessor;
    private readonly SortingManager _sortingManager;

    public string Name => "Processing and Sorting";

    public ProcessingAndSortingStage(PostProcessor postProcessor, SortingManager sortingManager)
    {
        _postProcessor = postProcessor;
        _sortingManager = sortingManager;
    }

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Handle processing of nodes with their associated candidates
        if (context.Metadata.TryGetValue("processedClasses", out var classesObj) &&
            classesObj is List<ProcessedClass> processedClasses)
        {
            // First sort the classes
            var sortedClasses = _sortingManager.SortClasses(processedClasses);

            // Then apply post-processing in sorted order
            var result = ImmutableList.CreateBuilder<AstNode>();

            foreach (var sortedClass in sortedClasses)
            {
                var processedNodes = _postProcessor.ApplyPostProcessing(
                    sortedClass.ProcessedClass.AstNodes,
                    sortedClass.ProcessedClass.Candidate);
                result.AddRange(processedNodes);
            }

            return result.ToImmutable();
        }

        return nodes;
    }
}