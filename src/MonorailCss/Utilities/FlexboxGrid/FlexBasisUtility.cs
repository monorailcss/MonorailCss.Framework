using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling the initial size of flex items.
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

    public string[] GetDocumentedProperties() => ["flex-basis"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("basis-auto", "Set flex basis to auto"),
            new Documentation.UtilityExample("basis-full", "Set flex basis to 100%"),
            new Documentation.UtilityExample("basis-1/2", "Set flex basis to 50%"),
            new Documentation.UtilityExample("basis-1/3", "Set flex basis to 33.333%"),
            new Documentation.UtilityExample("basis-min", "Set flex basis to min-content"),
            new Documentation.UtilityExample("basis-max", "Set flex basis to max-content"),
            new Documentation.UtilityExample("basis-fit", "Set flex basis to fit-content"),
            new Documentation.UtilityExample("basis-0", "Set flex basis to 0"),
            new Documentation.UtilityExample("basis-[200px]", "Use an arbitrary value for flex basis"),
        ];
    }
}