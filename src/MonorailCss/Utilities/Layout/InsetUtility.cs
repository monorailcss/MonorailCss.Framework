using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the placement of positioned elements.
/// </summary>
internal class InsetUtility : BaseFractionAwareSpacingUtility
{
    protected override string[] Patterns => ["inset", "top", "right", "bottom", "left", "start", "end"];

    protected override string[] SpacingNamespaces => ["--inset", "--spacing"];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "inset":
                // Use the shorthand inset property
                declarations.Add(new Declaration("inset", value, important));
                break;
            case "top":
                declarations.Add(new Declaration("top", value, important));
                break;
            case "right":
                declarations.Add(new Declaration("right", value, important));
                break;
            case "bottom":
                declarations.Add(new Declaration("bottom", value, important));
                break;
            case "left":
                declarations.Add(new Declaration("left", value, important));
                break;
            case "start":
                declarations.Add(new Declaration("inset-inline-start", value, important));
                break;
            case "end":
                declarations.Add(new Declaration("inset-inline-end", value, important));
                break;
        }

        return declarations.ToImmutableList();
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