using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline;

/// <summary>
/// Represents a single stage in the Monorail CSS pipeline process.
/// Each stage is responsible for performing a specific transformation or action
/// on the collection of AST (Abstract Syntax Tree) nodes.
/// </summary>
internal interface IPipelineStage
{
    /// <summary>
    /// Gets the name of the pipeline stage. This is a descriptive identifier
    /// used to distinguish and identify the specific transformation or operation
    /// performed by the stage during the pipeline execution.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Processes a collection of Abstract Syntax Tree (AST) nodes by applying transformations
    /// or actions defined by the implementing pipeline stage.
    /// </summary>
    /// <param name="nodes">
    /// The initial collection of AST nodes to be processed. These represent CSS structures
    /// or elements in the pipeline.
    /// </param>
    /// <param name="context">
    /// The context in which the pipeline stage operates. This provides necessary metadata or
    /// state for the transformation being applied.
    /// </param>
    /// <returns>
    /// A new immutable list of transformed AST nodes after the stage's processing logic
    /// has been applied.
    /// </returns>
    ImmutableList<AstNode> Process(
        ImmutableList<AstNode> nodes,
        PipelineContext context);
}