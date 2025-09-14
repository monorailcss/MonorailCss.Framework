using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utility for will-change property values.
/// Handles: will-change-auto, will-change-scroll, will-change-contents, will-change-transform, will-change-*
/// CSS: will-change: auto, will-change: scroll-position, will-change: contents, will-change: transform, etc.
/// </summary>
internal class WillChangeUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["will-change"];
    protected override string[] ThemeKeys => NamespaceResolver.WillChangeChain;

    /// <summary>
    /// Static will-change mappings for built-in values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _willChangeValues =
        new Dictionary<string, string>
        {
            ["auto"] = "auto",
            ["scroll"] = "scroll-position",
            ["contents"] = "contents",
            ["transform"] = "transform",
        }.ToImmutableDictionary();

    /// <summary>
    /// Handles bare values like "auto", "scroll", "contents", "transform" for predefined will-change values.
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle predefined will-change values
        if (_willChangeValues.TryGetValue(value, out var cssValue))
        {
            return cssValue;
        }

        // Let the base class handle theme resolution for other values
        return base.HandleBareValue(value);
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("will-change", value, important));
    }

    /// <summary>
    /// Validates arbitrary values for will-change property.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow valid CSS will-change values
        var validKeywords = new[]
        {
            "auto", "scroll-position", "contents", "transform",
            "opacity", "left", "top", "right", "bottom",
            "width", "height", "background-color", "color",
            "border-color", "box-shadow",
        };

        // Split by comma for multiple values and check each one
        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim());

        return values.All(v => validKeywords.Contains(v) ||
                              v.StartsWith("var(") ||
                              !string.IsNullOrWhiteSpace(v));
    }
}