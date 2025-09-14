using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utility for flex-basis values.
/// Handles: basis-auto, basis-full, basis-4, basis-1/2, basis-px, basis-[200px], etc.
/// CSS: flex-basis: auto, flex-basis: 100%, flex-basis: var(--spacing-4), flex-basis: 50%.
/// </summary>
internal class FlexBasisUtility : BaseFractionAwareSpacingUtility
{
    protected override string[] Patterns => ["basis"];

    protected override string[] SpacingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.FlexBasisChain, "--container");

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("flex-basis", value, important));
    }

    protected override bool TryResolveSpacing(CandidateValue value, Theme.Theme theme, out string spacing)
    {
        spacing = string.Empty;

        // Handle special static basis values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Static basis values that don't use spacing
            spacing = key switch
            {
                "auto" => "auto",
                "full" => "100%",
                "min" => "min-content",
                "max" => "max-content",
                "fit" => "fit-content",
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
                    spacing = $"calc({numerator}/{denominator} * 100%)";
                    return true;
                }
            }
        }

        // Fall back to base spacing resolution for numeric values and arbitrary values
        return base.TryResolveSpacing(value, theme, out spacing);
    }
}