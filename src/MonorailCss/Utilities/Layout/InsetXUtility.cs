using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles inset-x utilities for horizontal inset.
/// </summary>
internal class InsetXUtility : BaseFractionAwareSpacingUtility
{
    protected override string[] Patterns => ["inset-x"];

    protected override string[] SpacingNamespaces => ["--inset", "--spacing"];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("inset-inline", value, important));
    }

    protected override bool TryResolveSpacing(CandidateValue value, Theme.Theme theme, out string spacing)
    {
        spacing = string.Empty;

        // Handle special static inset values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Static inset values that don't use spacing
            spacing = key switch
            {
                "auto" => "auto",
                "full" => "100%",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(spacing))
            {
                return true;
            }

            // Handle fractions (e.g., "1/2", "1/3", "2/3", etc.)
            if (key.Contains('/'))
            {
                var parts = key.Split('/');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out var numerator) &&
                    int.TryParse(parts[1], out var denominator) &&
                    denominator > 0)
                {
                    // Negative values are handled by NegativeValueNormalizationStage
                    spacing = $"calc({numerator}/{denominator} * 100%)";
                    return true;
                }
            }
        }

        // Fall back to base spacing resolution for numeric values and arbitrary values
        return base.TryResolveSpacing(value, theme, out spacing);
    }
}