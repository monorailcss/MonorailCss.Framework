using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the size adjustment of text on mobile devices.
/// </summary>
internal class TextSizeAdjustUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["text-size-adjust"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("text-size-adjust", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle text-size-adjust keyword values
            resolvedValue = key switch
            {
                "none" => "none",
                "auto" => "auto",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }
        }

        // Handle arbitrary values (text-size-adjust-[120%])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow arbitrary percentage and keyword values
            if (IsValidTextSizeAdjustValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }
        }

        // No theme resolution needed for text-size-adjust
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid text-size-adjust value.
    /// </summary>
    private static bool IsValidTextSizeAdjustValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[]
        {
            "none", "auto", "inherit", "initial", "unset", "revert",
        };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow percentage values (typically 50% - 200%)
        if (value.EndsWith("%") && value.Length > 1)
        {
            var percentStr = value[..^1];
            if (float.TryParse(percentStr, out var percent) && percent >= 0)
            {
                return true;
            }
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidTextSizeAdjustValue(value);
    }
}