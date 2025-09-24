using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask linear gradient to position utilities.
/// Handles: mask-linear-to-0%, mask-linear-to-50%, mask-linear-to-100%, etc.
/// </summary>
internal class MaskLinearToUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["mask-linear-to"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Add the mask-image declaration with all three gradient variables
        declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));

        // Add mask-composite
        declarations.Add(new Declaration("mask-composite", "intersect", important));

        // Add the linear gradient stops
        declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));

        // Add the linear gradient
        declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));

        // Add the to position
        declarations.Add(new Declaration("--tw-mask-linear-to-position", value, important));

        return declarations.ToImmutable();
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle percentage values (e.g., 0%, 50%, 100%)
        if (value.Kind == ValueKind.Named && value.Value.EndsWith("%"))
        {
            resolvedValue = value.Value;
            return true;
        }

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            // Allow percentage values and CSS keywords
            if (IsValidPositionValue(value.Value))
            {
                resolvedValue = value.Value;
                return true;
            }
        }

        return false;
    }

    private static bool IsValidPositionValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow percentage values
        if (value.EndsWith("%"))
        {
            var percentStr = value[..^1];
            if (float.TryParse(percentStr, out var percent) && percent >= 0 && percent <= 100)
            {
                return true;
            }
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow keyword values
        var keywords = new[] { "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidPositionValue(value);
    }
}