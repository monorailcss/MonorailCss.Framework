using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for setting grid items to automatically span columns.
/// </summary>
internal class GridColumnAutoUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["col"];
    protected override string[] ThemeKeys => NamespaceResolver.GridColumnChain;
    protected override string? DefaultValue => null; // No default - only handles specific values

    protected override string? HandleBareValue(string value)
    {
        return value switch
        {
            "auto" => "auto",
            _ => null, // Only handle "auto" for col-auto
        };
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow grid line patterns like "1", "1 / 3", "span 2", "span 2 / span 2"
        if (value.Contains('/') || value.StartsWith("span ") || int.TryParse(value, out _))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("grid-column", value, important));
    }

    public string[]? GetDocumentedProperties() => ["grid-column"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return new[]
        {
            new Documentation.UtilityExample("col-auto", "Set grid column to auto"),
            new Documentation.UtilityExample("col-[1_/_3]", "Position element from column line 1 to 3"),
            new Documentation.UtilityExample("col-[span_2]", "Span 2 columns"),
        };
    }
}