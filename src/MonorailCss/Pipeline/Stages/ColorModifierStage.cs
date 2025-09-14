using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically applies opacity modifiers to color values
/// using color-mix() syntax. This centralizes opacity handling that was previously
/// duplicated across multiple color utilities.
/// </summary>
internal class ColorModifierStage : IPipelineStage
{
    private readonly Theme.Theme _theme;

    // Matches color properties that should have opacity modifiers applied
    private static readonly HashSet<string> _colorProperties =
    [
        "color",
        "background-color",
        "border-color",
        "border-top-color",
        "border-right-color",
        "border-bottom-color",
        "border-left-color",
        "outline-color",
        "text-decoration-color",
        "accent-color",
        "caret-color",
        "fill",
        "stroke",
        "--tw-gradient-from",
        "--tw-gradient-to",
        "--tw-gradient-via",
        "--tw-ring-color",
        "--tw-ring-offset-color",
        "--tw-divide-color",
        "--tw-shadow-color",
        "--tw-text-shadow-color",
    ];

    public string Name => "Color Modifier Application";

    public ColorModifierStage(Theme.Theme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get processed classes from context to access modifier information
        if (!context.Metadata.TryGetValue("processedClasses", out var classesObj) ||
            classesObj is not List<ProcessedClass> processedClasses)
        {
            return nodes;
        }

        // Process each class that has a modifier
        // Use ToList() to avoid collection modification errors
        var classesToProcess = processedClasses.ToList();
        for (var i = 0; i < classesToProcess.Count; i++)
        {
            var processedClass = classesToProcess[i];

            if (processedClass.Candidate.Modifier == null)
            {
                continue;
            }

            // Skip if this is not a color/opacity context
            // We'll let the modifier through and check if it resolves to an opacity

            // Apply opacity modifier to color declarations in this class's AST nodes
            var modifiedNodes = ApplyOpacityToNodes(
                processedClass.AstNodes,
                processedClass.Candidate.Modifier);

            // Update the processed class with modified nodes
            if (modifiedNodes != processedClass.AstNodes)
            {
                processedClasses[i] = processedClass with { AstNodes = modifiedNodes };
            }
        }

        // Return nodes unchanged - modifications are made to processedClasses in context
        return nodes;
    }

    private ImmutableList<AstNode> ApplyOpacityToNodes(ImmutableList<AstNode> nodes, Modifier modifier)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();
        var modified = false;

        foreach (var node in nodes)
        {
            var processedNode = ApplyOpacityToNode(node, modifier);
            builder.Add(processedNode);
            if (processedNode != node)
            {
                modified = true;
            }
        }

        return modified ? builder.ToImmutable() : nodes;
    }

    private AstNode ApplyOpacityToNode(AstNode node, Modifier modifier)
    {
        switch (node)
        {
            case Declaration declaration when IsColorProperty(declaration.Property):
                if (TryApplyOpacityModifier(declaration.Value, modifier, out var modifiedValue))
                {
                    return declaration with { Value = modifiedValue };
                }

                return declaration;

            case StyleRule styleRule:
                var modifiedStyleNodes = ApplyOpacityToNodes(styleRule.Nodes, modifier);
                return modifiedStyleNodes != styleRule.Nodes
                    ? styleRule with { Nodes = modifiedStyleNodes }
                    : styleRule;

            case NestedRule nestedRule:
                var modifiedNestedNodes = ApplyOpacityToNodes(nestedRule.Nodes, modifier);
                return modifiedNestedNodes != nestedRule.Nodes
                    ? nestedRule with { Nodes = modifiedNestedNodes }
                    : nestedRule;

            case AtRule atRule:
                var modifiedAtNodes = ApplyOpacityToNodes(atRule.Nodes, modifier);
                return modifiedAtNodes != atRule.Nodes
                    ? atRule with { Nodes = modifiedAtNodes }
                    : atRule;

            case Context contextNode:
                var modifiedContextNodes = ApplyOpacityToNodes(contextNode.Nodes, modifier);
                return modifiedContextNodes != contextNode.Nodes
                    ? contextNode with { Nodes = modifiedContextNodes }
                    : contextNode;

            default:
                return node;
        }
    }

    private static bool IsColorProperty(string property)
    {
        return _colorProperties.Contains(property);
    }

    private bool TryApplyOpacityModifier(
        string colorValue,
        Modifier modifier,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Don't modify values that already have color-mix applied
        if (colorValue.Contains("color-mix"))
        {
            return false;
        }

        // Don't modify inherit/initial/unset values, but DO allow transparent for opacity mixing
        if (colorValue is "inherit" or "initial" or "unset")
        {
            return false;
        }

        // Try to resolve the opacity value from the modifier
        if (!ValueResolver.TryResolveOpacity(modifier, _theme, out var opacity))
        {
            return false;
        }

        // Apply color-mix() to add opacity
        // Format: color-mix(in oklab, <color> <opacity>, transparent)
        result = $"color-mix(in oklab, {colorValue} {opacity}, transparent)";
        return true;
    }
}