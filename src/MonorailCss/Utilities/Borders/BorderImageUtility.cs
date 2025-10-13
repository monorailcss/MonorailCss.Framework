using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the border image of an element.
/// </summary>
internal class BorderImageUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["border-image"];

    protected override string[] ThemeKeys => [];

    protected override string? HandleBareValue(string value)
    {
        // Handle static values
        return value switch
        {
            "none" => "none",
            _ => null,
        };
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(new Declaration("border-image", value, important));
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

        // Allow url() functions
        if (value.StartsWith("url(") && value.EndsWith(")"))
        {
            return true;
        }

        // Allow gradient functions
        if (value.Contains("gradient("))
        {
            return true;
        }

        // Allow complex border-image values (source slice width outset repeat)
        // This is a basic validation - real CSS validation would be more complex
        return true;
    }
}