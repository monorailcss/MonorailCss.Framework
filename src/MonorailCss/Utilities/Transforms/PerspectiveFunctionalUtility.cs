using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the perspective of 3D transformed elements.
/// </summary>
internal class PerspectiveFunctionalUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["perspective"];

    protected override string[] ThemeKeys => []; // No theme resolution for perspective

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Only generate CSS for the perspective pattern
        if (pattern == "perspective")
        {
            return ImmutableList.Create<AstNode>(
                new Declaration("perspective", value, important));
        }

        return ImmutableList<AstNode>.Empty;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        // Basic validation - should be a length value
        return !string.IsNullOrEmpty(value) &&
               (value.EndsWith("px") || value.EndsWith("em") || value.EndsWith("rem") ||
                value.EndsWith("vw") || value.EndsWith("vh") || value.EndsWith("vmin") || value.EndsWith("vmax"));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "perspective: [value]";
}