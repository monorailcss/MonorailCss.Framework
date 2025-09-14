using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically generates CSS variable fallback values.
/// This centralizes fallback generation that was previously duplicated across utilities.
/// </summary>
internal partial class VariableFallbackStage : IPipelineStage
{
    public string Name => "CSS Variable Fallback Generation";

    // Default fallback values for common CSS variables
    private static readonly Dictionary<string, string> _defaultFallbacks = new()
    {
        // Shadow variables
        ["--tw-shadow-color"] = "rgb(0 0 0 / 0.1)",
        ["--tw-shadow"] = "0 0 transparent",
        ["--tw-inset-shadow"] = "0 0 transparent",
        ["--tw-inset-ring-shadow"] = "0 0 transparent",

        // Ring variables
        ["--tw-ring-color"] = "rgb(147 197 253 / 0.5)",
        ["--tw-ring-offset-color"] = "#fff",
        ["--tw-ring-offset-width"] = "0px",
        ["--tw-ring-shadow"] = "0 0 transparent",
        ["--tw-ring-offset-shadow"] = "0 0 transparent",

        // Transform variables
        ["--tw-rotate-x"] = string.Empty,
        ["--tw-rotate-y"] = string.Empty,
        ["--tw-rotate-z"] = string.Empty,
        ["--tw-skew-x"] = string.Empty,
        ["--tw-skew-y"] = string.Empty,
        ["--tw-scale-x"] = "1",
        ["--tw-scale-y"] = "1",
        ["--tw-translate-x"] = "0",
        ["--tw-translate-y"] = "0",

        // Touch action variables
        ["--tw-pan-x"] = string.Empty,
        ["--tw-pan-y"] = string.Empty,
        ["--tw-pinch-zoom"] = string.Empty,

        // Outline variables
        ["--tw-outline-style"] = "solid",

        // Filter and backdrop variables
        ["--tw-blur"] = string.Empty,
        ["--tw-brightness"] = string.Empty,
        ["--tw-contrast"] = string.Empty,
        ["--tw-grayscale"] = string.Empty,
        ["--tw-hue-rotate"] = string.Empty,
        ["--tw-invert"] = string.Empty,
        ["--tw-saturate"] = string.Empty,
        ["--tw-sepia"] = string.Empty,
        ["--tw-drop-shadow"] = string.Empty,
        ["--tw-backdrop-blur"] = string.Empty,
        ["--tw-backdrop-brightness"] = string.Empty,
        ["--tw-backdrop-contrast"] = string.Empty,
        ["--tw-backdrop-grayscale"] = string.Empty,
        ["--tw-backdrop-hue-rotate"] = string.Empty,
        ["--tw-backdrop-invert"] = string.Empty,
        ["--tw-backdrop-opacity"] = string.Empty,
        ["--tw-backdrop-saturate"] = string.Empty,
        ["--tw-backdrop-sepia"] = string.Empty,

        // Gradient variables
        ["--tw-gradient-from"] = "transparent",
        ["--tw-gradient-to"] = "transparent",
        ["--tw-gradient-via"] = "transparent",

        // Text shadow
        ["--tw-text-shadow-color"] = "rgb(0 0 0 / 0.5)",

        // Divide color
        ["--tw-divide-color"] = "#e5e7eb",
    };

    // Regex to find var() functions without fallbacks
    private static readonly Regex _varWithoutFallbackRegex = VarWithoutFallbackRegexDefinition();

    // Regex to find var() functions with empty fallbacks (like "var(--tw-rotate-x,)")
    private static readonly Regex _varWithEmptyFallbackRegex = VarWithEmptyFallbackRegexDefinition();

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Process all nodes to add fallbacks
        return ProcessNodes(nodes);
    }

    private ImmutableList<AstNode> ProcessNodes(ImmutableList<AstNode> nodes)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();
        var modified = false;

        foreach (var node in nodes)
        {
            var processedNode = ProcessNode(node);
            builder.Add(processedNode);
            if (processedNode != node)
            {
                modified = true;
            }
        }

        return modified ? builder.ToImmutable() : nodes;
    }

    private AstNode ProcessNode(AstNode node)
    {
        switch (node)
        {
            case Declaration declaration:
                if (TryAddFallbacksToValue(declaration.Value, out var modifiedValue))
                {
                    return declaration with { Value = modifiedValue };
                }

                return declaration;

            case StyleRule styleRule:
                var modifiedStyleNodes = ProcessNodes(styleRule.Nodes);
                return modifiedStyleNodes != styleRule.Nodes
                    ? styleRule with { Nodes = modifiedStyleNodes }
                    : styleRule;

            case NestedRule nestedRule:
                var modifiedNestedNodes = ProcessNodes(nestedRule.Nodes);
                return modifiedNestedNodes != nestedRule.Nodes
                    ? nestedRule with { Nodes = modifiedNestedNodes }
                    : nestedRule;

            case AtRule atRule:
                var modifiedAtNodes = ProcessNodes(atRule.Nodes);
                return modifiedAtNodes != atRule.Nodes
                    ? atRule with { Nodes = modifiedAtNodes }
                    : atRule;

            case Context contextNode:
                var modifiedContextNodes = ProcessNodes(contextNode.Nodes);
                return modifiedContextNodes != contextNode.Nodes
                    ? contextNode with { Nodes = modifiedContextNodes }
                    : contextNode;

            default:
                return node;
        }
    }

    private bool TryAddFallbacksToValue(string value, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Skip if no CSS variables are present
        if (!value.Contains("var("))
        {
            return false;
        }

        // Skip if value already has explicit fallbacks (not empty ones)
        // Check if all var() functions have non-empty fallbacks
        var hasProperFallbacks = true;
        var varMatches = VarMatchesRegexExpression().Matches(value);
        foreach (Match match in varMatches)
        {
            var varContent = match.Groups[1].Value;

            // If it has a comma but nothing after (or just whitespace), it needs a proper fallback
            if (varContent.Contains(','))
            {
                var parts = varContent.Split(',');
                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
                {
                    hasProperFallbacks = false;
                    break;
                }
            }
            else
            {
                // No comma means no fallback at all
                hasProperFallbacks = false;
                break;
            }
        }

        if (hasProperFallbacks)
        {
            return false;
        }

        var modifiedValue = value;
        var wasModified = false;

        // Replace var() functions without fallbacks
        modifiedValue = _varWithoutFallbackRegex.Replace(modifiedValue, match =>
        {
            var varName = match.Groups[1].Value;
            if (_defaultFallbacks.TryGetValue(varName, out var fallback))
            {
                wasModified = true;

                // For empty string fallbacks, use the empty comma pattern
                return string.IsNullOrEmpty(fallback)
                    ? $"var({varName},)"
                    : $"var({varName}, {fallback})";
            }

            return match.Value;
        });

        // Replace var() functions with empty fallbacks
        modifiedValue = _varWithEmptyFallbackRegex.Replace(modifiedValue, match =>
        {
            var varName = match.Groups[1].Value;
            if (_defaultFallbacks.TryGetValue(varName, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                wasModified = true;
                return $"var({varName}, {fallback})";
            }

            return match.Value;
        });

        if (wasModified)
        {
            result = modifiedValue;
            return true;
        }

        return false;
    }

    [GeneratedRegex(@"var\(([^)]+)\)")]
    private static partial Regex VarMatchesRegexExpression();
    [GeneratedRegex(@"var\((--tw-[^,)]+)\)", RegexOptions.Compiled)]
    private static partial Regex VarWithoutFallbackRegexDefinition();
    [GeneratedRegex(@"var\((--tw-[^,)]+),\s*\)", RegexOptions.Compiled)]
    private static partial Regex VarWithEmptyFallbackRegexDefinition();
}