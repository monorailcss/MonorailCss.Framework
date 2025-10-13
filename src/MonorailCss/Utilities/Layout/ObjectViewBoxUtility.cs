using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the view box of a replaced element.
/// </summary>
internal class ObjectViewBoxUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["object-view-box"];

    protected override string[] ThemeKeys => [];

    protected override string? HandleBareValue(string value)
    {
        // object-view-box doesn't have standard named values, only arbitrary ones
        return null;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(new Declaration("object-view-box", value, important));
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow inset() function
        if (value.StartsWith("inset(") && value.EndsWith(")"))
        {
            return true;
        }

        // Allow rect() function
        if (value.StartsWith("rect(") && value.EndsWith(")"))
        {
            return true;
        }

        // Allow other geometric functions that might be valid for object-view-box
        if (value.StartsWith("circle(") || value.StartsWith("ellipse(") || value.StartsWith("polygon("))
        {
            return true;
        }

        return false;
    }
}