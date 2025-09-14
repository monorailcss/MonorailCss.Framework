using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically normalizes negative values in CSS declarations.
/// This centralizes negative value handling that was previously duplicated across
/// multiple spacing, positioning, and transform utilities.
/// </summary>
internal partial class NegativeValueNormalizationStage : IPipelineStage
{
    // Properties that should have negative value normalization applied
    private static readonly HashSet<string> _negativeCapableProperties = new()
    {
        // Spacing properties
        "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
        "margin-inline", "margin-inline-start", "margin-inline-end",
        "margin-block", "margin-block-start", "margin-block-end",

        // Positioning properties
        "top", "right", "bottom", "left",
        "inset", "inset-inline", "inset-inline-start", "inset-inline-end",
        "inset-block", "inset-block-start", "inset-block-end",

        // Transform properties - NOTE: NOT "translate" itself, only the variables
        "--tw-translate-x", "--tw-translate-y", "--tw-translate-z",
        "--tw-rotate", "--tw-skew-x", "--tw-skew-y",

        // Grid/Flex properties
        "order", "z-index",

        // Scroll properties
        "scroll-margin", "scroll-margin-top", "scroll-margin-right",
        "scroll-margin-bottom", "scroll-margin-left",
        "scroll-margin-inline", "scroll-margin-inline-start", "scroll-margin-inline-end",
        "scroll-margin-block", "scroll-margin-block-start", "scroll-margin-block-end",

        // Other properties that accept negative values
        "text-indent", "letter-spacing", "word-spacing",
    };

    public string Name => "Negative Value Normalization";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get processed classes from context
        if (!context.Metadata.TryGetValue("processedClasses", out var classesObj) ||
            classesObj is not List<ProcessedClass> processedClasses)
        {
            return nodes;
        }

        // Process each class that might have negative values
        // Use ToList() to avoid collection modification errors
        var classesToProcess = processedClasses.ToList();
        for (var i = 0; i < classesToProcess.Count; i++)
        {
            var processedClass = classesToProcess[i];

            // Check if the utility (not the full class name) starts with a negative sign
            // This handles cases like "lg:-start-4" where the negative is after the variant
            var isNegativeClass = false;

            if (processedClass.Candidate is FunctionalUtility functionalUtility)
            {
                isNegativeClass = functionalUtility.Root.StartsWith('-') &&
                                !functionalUtility.Root.StartsWith("--"); // Avoid CSS variables
            }
            else if (processedClass.Candidate is StaticUtility staticUtility)
            {
                isNegativeClass = staticUtility.Root.StartsWith('-') &&
                                !staticUtility.Root.StartsWith("--");
            }

            if (!isNegativeClass)
            {
                continue;
            }

            // Apply negative normalization to declarations in this class's AST nodes
            var normalizedNodes = NormalizeNegativeValues(processedClass.AstNodes, processedClass.Candidate);

            // Update the processed class with normalized nodes
            if (normalizedNodes != processedClass.AstNodes)
            {
                processedClasses[i] = processedClass with { AstNodes = normalizedNodes };
            }
        }

        // Return nodes unchanged - modifications are made to processedClasses in context
        return nodes;
    }

    private ImmutableList<AstNode> NormalizeNegativeValues(ImmutableList<AstNode> nodes, Candidate candidate)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();
        var modified = false;

        foreach (var node in nodes)
        {
            var processedNode = NormalizeNegativeNode(node, candidate);
            builder.Add(processedNode);
            if (processedNode != node)
            {
                modified = true;
            }
        }

        return modified ? builder.ToImmutable() : nodes;
    }

    private AstNode NormalizeNegativeNode(AstNode node, Candidate candidate)
    {
        switch (node)
        {
            case Declaration declaration when ShouldNormalizeNegative(declaration.Property):
                if (TryNormalizeNegativeValue(declaration.Value, candidate, out var normalizedValue))
                {
                    return declaration with { Value = normalizedValue };
                }

                return declaration;

            case StyleRule styleRule:
                var normalizedStyleNodes = NormalizeNegativeValues(styleRule.Nodes, candidate);
                return normalizedStyleNodes != styleRule.Nodes
                    ? styleRule with { Nodes = normalizedStyleNodes }
                    : styleRule;

            case NestedRule nestedRule:
                var normalizedNestedNodes = NormalizeNegativeValues(nestedRule.Nodes, candidate);
                return normalizedNestedNodes != nestedRule.Nodes
                    ? nestedRule with { Nodes = normalizedNestedNodes }
                    : nestedRule;

            case AtRule atRule:
                var normalizedAtNodes = NormalizeNegativeValues(atRule.Nodes, candidate);
                return normalizedAtNodes != atRule.Nodes
                    ? atRule with { Nodes = normalizedAtNodes }
                    : atRule;

            case Context contextNode:
                var normalizedContextNodes = NormalizeNegativeValues(contextNode.Nodes, candidate);
                return normalizedContextNodes != contextNode.Nodes
                    ? contextNode with { Nodes = normalizedContextNodes }
                    : contextNode;

            default:
                return node;
        }
    }

    private static bool ShouldNormalizeNegative(string property)
    {
        return _negativeCapableProperties.Contains(property);
    }

    private bool TryNormalizeNegativeValue(
        string value,
        Candidate candidate,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Skip if already normalized (contains calc with negative)
        if (value.Contains("calc(-") || value.Contains("calc( -"))
        {
            return false;
        }

        // Skip special values
        if (value is "auto" or "inherit" or "initial" or "unset" or "0")
        {
            return false;
        }

        // Check if the candidate indicates this should be negative
        var isNegative = false;

        if (candidate is FunctionalUtility functionalUtility)
        {
            isNegative = functionalUtility.Root.StartsWith('-') &&
                        !functionalUtility.Root.StartsWith("--"); // Avoid CSS variables
        }
        else if (candidate is StaticUtility staticUtility)
        {
            isNegative = staticUtility.Root.StartsWith('-') &&
                        !staticUtility.Root.StartsWith("--");
        }

        if (!isNegative)
        {
            return false;
        }

        // For values that are already negative numbers, don't double-negate
        if (value.StartsWith('-'))
        {
            return false;
        }

        // For calc expressions that already multiply by a negative, skip
        if (value.Contains("* -") || value.Contains("*-"))
        {
            return false;
        }

        // Apply negative transformation based on value type
        if (IsNumericValue(value))
        {
            // Direct numeric values: just add negative sign
            result = $"-{value}";
        }
        else if (value.StartsWith("var("))
        {
            // CSS variables: wrap in calc for negation
            result = $"calc({value} * -1)";
        }
        else if (value.StartsWith("calc("))
        {
            // Existing calc: try to merge the negation intelligently
            // If it's calc(var(...) * N), change to calc(var(...) * -N)
            var calcContent = value.Substring(5, value.Length - 6); // Remove "calc(" and ")"

            // Check for simple multiplication pattern: var(...) * number
            var match = MultipleRegex().Match(calcContent);

            if (match.Success)
            {
                // Simple multiplication - just negate the number
                var varPart = match.Groups[1].Value;
                var numPart = match.Groups[2].Value;
                result = $"calc({varPart} * -{numPart})";
            }
            else
            {
                // Complex calc - wrap and negate
                result = $"calc({value} * -1)";
            }
        }
        else if (ContainsNumericValue(value))
        {
            // Other values with numbers: wrap in calc
            result = $"calc({value} * -1)";
        }
        else
        {
            // Unknown format, don't modify
            return false;
        }

        return true;
    }

    private static bool IsNumericValue(string value)
    {
        // Match numeric values with optional units
        return IsNumericRegex().IsMatch(value);
    }

    private static bool ContainsNumericValue(string value)
    {
        // Check if the value contains any numeric component
        return ContainsNumericRegex().IsMatch(value);
    }

    [GeneratedRegex(@"^(var\([^)]+\))\s*\*\s*(\d+(?:\.\d+)?)$")]
    private static partial Regex MultipleRegex();
    [GeneratedRegex(@"^[\d.]+(%|px|em|rem|vh|vw|vmin|vmax|ex|ch|cm|mm|in|pt|pc)?$")]
    private static partial Regex IsNumericRegex();
    [GeneratedRegex(@"[\d.]+")]
    private static partial Regex ContainsNumericRegex();
}