using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically applies important flags to declarations
/// based on the candidate's important modifier. This centralizes important flag
/// handling that was previously duplicated across all utilities.
/// </summary>
internal class ImportantFlagStage : IPipelineStage
{
    public string Name => "Important Flag Propagation";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get processed classes from context to access candidate information
        if (!context.Metadata.TryGetValue("processedClasses", out var classesObj) ||
            classesObj is not List<ProcessedClass> processedClasses)
        {
            return nodes;
        }

        // Process each class that has the important flag
        var classesToProcess = processedClasses.ToList();
        for (var i = 0; i < classesToProcess.Count; i++)
        {
            var processedClass = classesToProcess[i];

            if (!processedClass.Candidate.Important)
            {
                continue;
            }

            // Apply important flag to declarations in this class's AST nodes
            var modifiedNodes = ApplyImportantToNodes(processedClass.AstNodes, true);

            // Update the processed class with modified nodes
            if (modifiedNodes != processedClass.AstNodes)
            {
                processedClasses[i] = processedClass with { AstNodes = modifiedNodes };
            }
        }

        return nodes;
    }

    private ImmutableList<AstNode> ApplyImportantToNodes(ImmutableList<AstNode> nodes, bool important)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();
        var modified = false;

        foreach (var node in nodes)
        {
            var processedNode = ApplyImportantToNode(node, important);
            builder.Add(processedNode);
            if (processedNode != node)
            {
                modified = true;
            }
        }

        return modified ? builder.ToImmutable() : nodes;
    }

    private AstNode ApplyImportantToNode(AstNode node, bool important)
    {
        switch (node)
        {
            case Declaration declaration:
                // Apply the important flag
                return declaration with { Important = important };

            case StyleRule styleRule:
                var modifiedStyleNodes = ApplyImportantToNodes(styleRule.Nodes, important);
                return modifiedStyleNodes != styleRule.Nodes
                    ? styleRule with { Nodes = modifiedStyleNodes }
                    : styleRule;

            case NestedRule nestedRule:
                var modifiedNestedNodes = ApplyImportantToNodes(nestedRule.Nodes, important);
                return modifiedNestedNodes != nestedRule.Nodes
                    ? nestedRule with { Nodes = modifiedNestedNodes }
                    : nestedRule;

            case AtRule atRule:
                var modifiedAtNodes = ApplyImportantToNodes(atRule.Nodes, important);
                return modifiedAtNodes != atRule.Nodes
                    ? atRule with { Nodes = modifiedAtNodes }
                    : atRule;

            case Context contextNode:
                var modifiedContextNodes = ApplyImportantToNodes(contextNode.Nodes, important);
                return modifiedContextNodes != contextNode.Nodes
                    ? contextNode with { Nodes = modifiedContextNodes }
                    : contextNode;

            default:
                return node;
        }
    }
}